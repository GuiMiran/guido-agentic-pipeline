"""Bounded data-only ZIP and source-custody validation. Never execute source files."""
import argparse
import hashlib
import json
from pathlib import Path
import re
import stat
import zipfile

NAMES = {'expected.json', 'spectra-expected.json', 'acceptance-links.json', 'summary.json', 'trx/tests.trx'}


def unique_object(pairs):
    result = {}
    for key, value in pairs:
        if key in result:
            raise ValueError('Duplicate JSON key')
        result[key] = value
    return result


def load_json(data):
    if len(data) > 2 * 1024 * 1024:
        raise ValueError('JSON size limit')
    return json.loads(data, object_pairs_hook=unique_object)


def validate_documents(documents, custody, trx):
    source = custody['source']
    if custody.get('schemaVersion') != 1 or custody.get('acquisition') != 'authenticated-github-actions-api' or \
       source.get('repository') != 'GuiMiran/guido-agentic-pipeline' or source.get('repositoryId') != 1190006301 or \
       source.get('workflowId') != 250433858 or not re.fullmatch(r'[a-f0-9]{40}', source.get('commit', '')) or \
       source.get('conclusion') not in ['success', 'failure'] or not isinstance(source.get('runId'), int) or \
       source['runId'] < 1 or not isinstance(source.get('attempt'), int) or source['attempt'] < 1:
        raise ValueError('Invalid custody source')
    expected, canonical, ledger, summary = (documents[name] for name in
        ['expected.json', 'spectra-expected.json', 'acceptance-links.json', 'summary.json'])
    commit = source['commit']
    if any(doc.get('commit') != commit for doc in [expected, ledger, summary]) or \
       canonical.get('source') != {'system': 'guido-agentic-pipeline/declared-acceptance-map', 'revision': commit}:
        raise ValueError('Inventory/summary commit mismatch')
    manifest = load_json(custody['documents']['specs/spectra-mapping.json'])
    criteria = re.findall(r'^## (AC-\d{3}) — .+$', custody['documents']['.spectra/11-acceptance-criteria.md'], re.M)
    rows = manifest.get('mappings')
    if manifest.get('schemaVersion') != 1 or manifest.get('requirementsFile') != '.spectra/11-acceptance-criteria.md' or \
       not isinstance(rows, list) or not rows or len(rows) > 1000 or len(criteria) != len(set(criteria)):
        raise ValueError('Invalid versioned source mapping')
    for field in ['testId', 'name', 'acceptanceId']:
        values = [row.get(field) for row in rows]
        if any(not isinstance(value, str) or not value for value in values) or len(set(values)) != len(values):
            raise ValueError('Duplicate/invalid source test identities')
    wanted_native = [{'id': row['testId'], 'name': row['name']} for row in rows]
    # Domain inventory's original traversal order need not be the mapping order.
    if expected.get('schemaVersion') != 1 or not isinstance(expected.get('tests'), list) or \
       sorted(expected['tests'], key=lambda item: item['id']) != sorted(wanted_native, key=lambda item: item['id']):
        raise ValueError('Native inventory differs from authenticated versioned mapping')
    wanted_canonical = [{'id': row['acceptanceId'], 'key': row['name'], 'execution': 'expected',
                         'pipelineTestId': row['testId']} for row in rows]
    if canonical.get('schemaVersion') != 1 or canonical.get('tests') != wanted_canonical:
        raise ValueError('Canonical inventory differs from authenticated versioned mapping')
    files = custody.get('files')
    if not isinstance(files, list) or not files or len(files) > 100 or any(set(item) != {'file', 'sha256'} or
       not re.fullmatch(r'[a-f0-9]{64}', item['sha256']) for item in files) or \
       len({item['file'] for item in files}) != len(files):
        raise ValueError('Invalid authenticated source index')
    source_digest = hashlib.sha256(json.dumps(files, sort_keys=True, separators=(',', ':')).encode()).hexdigest()
    if canonical.get('mapping', {}).get('files') != files or canonical['mapping'].get('sourceDigest') != source_digest or \
       ledger.get('sourceDigest') != source_digest:
        raise ValueError('Source dependency hashes differ from authenticated Git blobs')
    for name, text in custody['documents'].items():
        record = next((item for item in files if item['file'] == name), None)
        if not record or record['sha256'] != hashlib.sha256(text.encode()).hexdigest():
            raise ValueError('Source document hash mismatch')
    mapped = {row['acceptanceId'] for row in rows}
    if not mapped.issubset(set(criteria)) or ledger.get('declaredCriteria') != len(criteria) or \
       ledger.get('linkedCriteria') != len(rows) or ledger.get('unassessedCriteria') != sorted(set(criteria) - mapped) or \
       ledger.get('behaviourallyVerified', 'missing') is not None:
        raise ValueError('Acceptance scope ledger mismatch')
    if summary.get('sources') != [{'file': 'tests.trx', 'sha256': hashlib.sha256(trx).hexdigest()}]:
        raise ValueError('TRX hash differs from native summary')
    # Never trust summary counts as the execution oracle: SPECTRA reads original TRX.
    return source_digest


def unpack(bundle, destination):
    bundle, destination = Path(bundle), Path(destination)
    if bundle.is_symlink() or {file.name for file in bundle.iterdir()} != {'custody.json', 'source.zip'} or \
       (bundle / 'custody.json').is_symlink():
        raise ValueError('Unsafe custody bundle')
    custody = load_json((bundle / 'custody.json').read_bytes())
    archive = bundle / 'source.zip'
    if archive.is_symlink() or archive.stat().st_size > 8 * 1024 * 1024:
        raise ValueError('Unsafe/oversized archive')
    raw = archive.read_bytes()
    if len(raw) != custody['artifact']['size'] or \
       'sha256:' + hashlib.sha256(raw).hexdigest() != custody['artifact']['digest']:
        raise ValueError('Archive custody digest mismatch')
    content = {}
    with zipfile.ZipFile(archive) as zipped:
        entries = zipped.infolist()
        if len(entries) != len(NAMES) or {entry.filename for entry in entries} != NAMES:
            raise ValueError('Missing, duplicate or unexpected archive member')
        if sum(entry.file_size for entry in entries) > 32 * 1024 * 1024:
            raise ValueError('Expanded archive size limit')
        for entry in entries:
            mode = stat.S_IFMT(entry.external_attr >> 16)
            limit = 16 * 1024 * 1024 if entry.filename.endswith('.trx') else 2 * 1024 * 1024
            if mode not in [0, stat.S_IFREG] or entry.flag_bits & 1 or entry.orig_filename != entry.filename or \
               entry.file_size < 1 or entry.file_size > limit or \
               entry.compress_type not in [zipfile.ZIP_STORED, zipfile.ZIP_DEFLATED]:
                raise ValueError('Unsafe or oversized archive member')
            content[entry.filename] = zipped.read(entry)
    documents = {name: load_json(data) for name, data in content.items() if name.endswith('.json')}
    validate_documents(documents, custody, content['trx/tests.trx'])
    destination.mkdir(mode=0o700, parents=False, exist_ok=False)
    for name, data in content.items():
        file = destination / name  # Names are the fixed allow-list above, no arbitrary archive paths.
        file.parent.mkdir(exist_ok=True)
        with file.open('xb') as stream:
            stream.write(data)
    return custody


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('--bundle', required=True)
    parser.add_argument('--destination', required=True)
    args = parser.parse_args()
    unpack(args.bundle, args.destination)
