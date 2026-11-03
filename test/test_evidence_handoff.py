"""Adversarial archive and provenance controls, stdlib only."""
import copy
import hashlib
import io
import json
from pathlib import Path
import stat
import sys
import tempfile
import unittest
import zipfile

sys.path.insert(0, str(Path(__file__).resolve().parents[1] / 'scripts'))
from evidence_handoff import unpack, load_json, validate_documents


def fixtures():
    commit = 'a' * 40
    rows = [{'testId': 'AUTH-001', 'name': 'One', 'acceptanceId': 'AC-001'}]
    manifest = json.dumps({'schemaVersion': 1, 'requirementsFile': '.spectra/11-acceptance-criteria.md', 'mappings': rows})
    criteria = '# Scope\n## AC-001 — One\n'
    source_documents = {'specs/spectra-mapping.json': manifest, '.spectra/11-acceptance-criteria.md': criteria}
    files = [{'file': name, 'sha256': hashlib.sha256(text.encode()).hexdigest()} for name, text in sorted(source_documents.items())]
    digest = hashlib.sha256(json.dumps(files, sort_keys=True, separators=(',', ':')).encode()).hexdigest()
    trx = b'<TestRun/>'  # ZIP layer does not claim XML validity; SPECTRA owns it.
    documents = {
        'expected.json': {'schemaVersion': 1, 'commit': commit, 'tests': [{'id': 'AUTH-001', 'name': 'One'}]},
        'spectra-expected.json': {'schemaVersion': 1,
            'source': {'system': 'guido-agentic-pipeline/declared-acceptance-map', 'revision': commit},
            'tests': [{'id': 'AC-001', 'key': 'One', 'execution': 'expected', 'pipelineTestId': 'AUTH-001'}],
            'mapping': {'files': files, 'sourceDigest': digest}},
        'acceptance-links.json': {'commit': commit, 'sourceDigest': digest, 'declaredCriteria': 1,
            'linkedCriteria': 1, 'unassessedCriteria': [], 'behaviourallyVerified': None},
        'summary.json': {'commit': commit, 'sources': [{'file': 'tests.trx', 'sha256': hashlib.sha256(trx).hexdigest()}]},
    }
    custody = {'schemaVersion': 1, 'acquisition': 'authenticated-github-actions-api', 'mode': 'pull_request',
        'source': {'repository': 'GuiMiran/guido-agentic-pipeline', 'repositoryId': 1190006301,
                   'workflowId': 250433858, 'runId': 42, 'attempt': 1, 'commit': commit, 'conclusion': 'success'},
        'files': files, 'documents': source_documents}
    return documents, custody, trx


class HandoffTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory(prefix='handoff-zip-')
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        self.bundle = self.root / 'bundle'
        self.bundle.mkdir()
        self.documents, self.custody, self.trx = fixtures()

    def write_bundle(self, mutate_entries=None, mutate_raw=None):
        entries = [(name, json.dumps(document).encode(), 0) for name, document in self.documents.items()]
        entries.append(('trx/tests.trx', self.trx, 0))
        if mutate_entries:
            entries = mutate_entries(entries)
        data = io.BytesIO()
        with zipfile.ZipFile(data, 'w', compression=zipfile.ZIP_DEFLATED) as archive:
            for name, content, mode in entries:
                entry = zipfile.ZipInfo(name)
                entry.compress_type = zipfile.ZIP_DEFLATED
                if mode:
                    entry.external_attr = mode << 16
                archive.writestr(entry, content)
        raw = data.getvalue()
        if mutate_raw:
            raw = mutate_raw(raw)
        self.custody['artifact'] = {'size': len(raw), 'digest': 'sha256:' + hashlib.sha256(raw).hexdigest()}
        (self.bundle / 'source.zip').write_bytes(raw)
        (self.bundle / 'custody.json').write_text(json.dumps(self.custody))

    def test_safe_members_are_validated_before_exclusive_extraction(self):
        self.write_bundle()
        output = self.root / 'input'
        unpack(self.bundle, output)
        self.assertEqual((output / 'trx/tests.trx').read_bytes(), self.trx)
        self.assertEqual(len(list(output.rglob('*'))), 6)  # five files + trx directory
        with self.assertRaises(FileExistsError):
            unpack(self.bundle, output)

    def test_paths_extra_members_missing_and_duplicate_files_are_rejected(self):
        for mutate in [lambda entries: entries[:-1], lambda entries: entries + [entries[0]],
                       lambda entries: entries + [('../outside', b'bad', 0)],
                       lambda entries: entries + [('evil.cjs', b'require("fs")', 0)],
                       lambda entries: [(name.replace('/', '\\'), content, mode) for name, content, mode in entries]]:
            self.write_bundle(mutate)
            output = self.root / 'blocked'
            with self.assertRaises(ValueError):
                unpack(self.bundle, output)
            self.assertFalse(output.exists())

    def test_symlink_member_is_never_followed(self):
        self.write_bundle(lambda entries: [(name, data, stat.S_IFLNK | 0o777) for name, data, _ in entries])
        with self.assertRaisesRegex(ValueError, 'Unsafe'):
            unpack(self.bundle, self.root / 'blocked')

    def test_digest_and_symlink_archive_fail(self):
        self.write_bundle()
        file = self.bundle / 'source.zip'
        file.write_bytes(file.read_bytes() + b'changed')
        with self.assertRaisesRegex(ValueError, 'digest'):
            unpack(self.bundle, self.root / 'blocked')
        file.rename(self.bundle / 'target.zip')
        file.symlink_to(self.bundle / 'target.zip')
        with self.assertRaisesRegex(ValueError, 'Unsafe'):
            unpack(self.bundle, self.root / 'blocked')

    def test_oversized_and_empty_entries_fail_before_output(self):
        for content in [b'', b'x' * (16 * 1024 * 1024 + 1)]:
            self.write_bundle(lambda entries: [(name, content if name.endswith('.trx') else data, mode)
                                               for name, data, mode in entries])
            with self.assertRaisesRegex(ValueError, 'oversized'):
                unpack(self.bundle, self.root / 'blocked')
            self.assertFalse((self.root / 'blocked').exists())

    def test_duplicate_json_keys_and_invalid_json_fail(self):
        for text in [b'{"commit":"one","commit":"two"}', b'{bad', b'x' * (2 * 1024 * 1024 + 1)]:
            with self.assertRaises(ValueError):
                load_json(text)

    def test_all_artifact_commit_fields_must_match_authenticated_run(self):
        for name in self.documents:
            docs = copy.deepcopy(self.documents)
            if name == 'spectra-expected.json':
                docs[name]['source']['revision'] = 'b' * 40
            else:
                docs[name]['commit'] = 'b' * 40
            with self.subTest(name=name), self.assertRaisesRegex(ValueError, 'commit'):
                validate_documents(docs, self.custody, self.trx)

    def test_expected_inventory_cannot_be_rewritten_from_result_data(self):
        for name in ['expected.json', 'spectra-expected.json']:
            docs = copy.deepcopy(self.documents)
            docs[name]['tests'] = []
            with self.assertRaisesRegex(ValueError, 'inventory'):
                validate_documents(docs, self.custody, self.trx)

    def test_source_dependency_index_and_original_trx_hash_are_authoritative(self):
        for mutate in [lambda docs: docs['spectra-expected.json']['mapping']['files'].clear(),
                       lambda docs: docs['acceptance-links.json'].update(sourceDigest='0' * 64),
                       lambda docs: docs['summary.json']['sources'][0].update(sha256='0' * 64)]:
            docs = copy.deepcopy(self.documents)
            mutate(docs)
            with self.assertRaises(ValueError):
                validate_documents(docs, self.custody, self.trx)

    def test_unassessed_obligations_and_static_ledger_cannot_be_hidden(self):
        for change in [{'unassessedCriteria': ['AC-999']}, {'declaredCriteria': 0},
                       {'linkedCriteria': 100}, {'behaviourallyVerified': True}]:
            docs = copy.deepcopy(self.documents)
            docs['acceptance-links.json'].update(change)
            with self.assertRaisesRegex(ValueError, 'scope ledger'):
                validate_documents(docs, self.custody, self.trx)


if __name__ == '__main__':
    unittest.main()
