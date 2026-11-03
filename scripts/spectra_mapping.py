"""Freeze declared scenario-to-acceptance links, never inferred behavioural truth.

Standard library only. The owner reviews the semantic mapping in the PR. This
validator checks referential integrity and binds source bytes; it cannot prove
that an assertion implements every part of a business rule.
"""
import argparse
from collections import Counter
import hashlib
import json
from pathlib import Path
import re

from evidence_gate import inventory


REQUIREMENTS = '.spectra/11-acceptance-criteria.md'
MANIFEST = 'specs/spectra-mapping.json'
TESTS = 'src/GUIDO.Agentic.Tests/Tests/ScenarioTests.cs'
REVIEWED_IDS = {f'AC-{i:03}' for i in range(1, 29)}
KNOWN_UNASSESSED = {'AC-025'}  # Description selector still requires owner-approved documentation.
FACT = re.compile(r'\[Fact\(DisplayName = "([^"]+)"\), Trait\("Spec", "([^"]+)"\)\]\s+public void (\w+)\(')


def read_source(root, relative):
    """Never resolve manifest-supplied traversal or symbolic links."""
    if not isinstance(relative, str) or '\\' in relative:
        raise ValueError('Invalid source path')
    parts = Path(relative).parts
    if not parts or Path(relative).is_absolute() or '..' in parts:
        raise ValueError('Unsafe source path')
    current = root
    for part in parts:
        current = current / part
        if current.is_symlink():
            raise ValueError('Symlink source is not supported')
    if not current.is_file() or current.stat().st_size > 2 * 1024 * 1024:
        raise ValueError('Missing or oversized mapping source')
    return current.read_bytes()


def build_mapping(root, commit):
    root = Path(root).resolve()
    if not isinstance(commit, str) or not re.fullmatch(r'(?:[0-9a-fA-F]{40}|[0-9a-fA-F]{64})', commit):
        raise ValueError('A full source commit is required')
    files = {REQUIREMENTS, MANIFEST, TESTS}
    manifest = json.loads(read_source(root, MANIFEST))
    if not isinstance(manifest, dict) or manifest.get('schemaVersion') != 1 or manifest.get('requirementsFile') != REQUIREMENTS:
        raise ValueError('Invalid mapping version or canonical requirements path')
    requirements = read_source(root, REQUIREMENTS).decode('utf-8')
    headings = re.findall(r'^## (AC-\d{3}) — .+$', requirements, re.M)
    if set(headings) != REVIEWED_IDS or len(headings) != len(REVIEWED_IDS):
        raise ValueError('Missing, duplicate or unexpected acceptance criteria in reviewed 28-ID scope')
    for ident in KNOWN_UNASSESSED:
        section = re.search(rf'^## {ident} — .*?(?=^## |\Z)', requirements, re.M | re.S)
        if not section or 'Status: UNASSESSED' not in section.group():
            raise ValueError('Known unassessed obligations must remain visible')

    # Inventory is independently sourced from original feature documentation.
    # Validate source paths before the legacy inventory scanner reads them.
    for directory in ['specs', 'src/GUIDO.Agentic.Tests/Tests', 'src/GUIDO.Agentic.Tests/Pages']:
        current = root
        for part in Path(directory).parts:
            current /= part
            if current.is_symlink():
                raise ValueError('Symlink source directory is not supported')
    scenarios = {}
    for file in sorted((root / 'specs').rglob('*')):
        if file.is_symlink():
            raise ValueError('Symlink specification is not supported')
        if file.is_file() and (file.suffix == '.feature' or file.name.endswith('.context.md')):
            relative = file.relative_to(root).as_posix()
            content = read_source(root, relative).decode('utf-8')
            files.add(relative)
            if file.suffix == '.feature':
                for name in re.findall(r'^\s*Scenario:\s*(.+?)\s*$', content, re.M):
                    if name in scenarios:
                        raise ValueError('Duplicate documented scenario')
                    scenarios[name] = relative
    for file in sorted((root / 'src/GUIDO.Agentic.Tests').rglob('*.cs')):
        if 'bin' not in file.parts and 'obj' not in file.parts:
            relative = file.relative_to(root).as_posix()
            read_source(root, relative)
            files.add(relative)
    for ident in headings:
        section = re.search(rf'^## {ident} — .*?(?=^## |\Z)', requirements, re.M | re.S).group()
        references = re.findall(r'^Source: `([^`]+)`', section, re.M)
        if len(references) != 1 or references[0] not in files or not references[0].startswith('specs/'):
            raise ValueError('Acceptance source reference is missing or unresolved')
    expected = inventory(root)
    facts = {}
    for test_file in sorted((root / 'src/GUIDO.Agentic.Tests/Tests').glob('*.cs')):
        code = read_source(root, test_file.relative_to(root).as_posix()).decode('utf-8')
        for match in FACT.finditer(code):
            classes = re.findall(r'public class (\w+) : BaseTest', code[:match.start()])
            if not classes or match[2] in facts:
                raise ValueError('Invalid or duplicate Fact identity')
            facts[match[2]] = (match[1], f'{classes[-1]}.{match[3]}')
    rows = manifest.get('mappings')
    if not isinstance(rows, list) or len(rows) != len(expected) or any(not isinstance(row, dict) for row in rows):
        raise ValueError('Exactly one mapping per independent scenario is required')
    for field in ['testId', 'name', 'acceptanceId']:
        values = [row.get(field) for row in rows]
        if any(not isinstance(value, str) or not value for value in values) or any(count != 1 for count in Counter(values).values()):
            raise ValueError(f'Missing or duplicate mapping {field}')
    if {row['testId']: row['name'] for row in rows} != {item['id']: item['name'] for item in expected}:
        raise ValueError('Mapping differs from independent test inventory')
    if {row['acceptanceId'] for row in rows} != REVIEWED_IDS - KNOWN_UNASSESSED:
        raise ValueError('Mapping must preserve 27 scoped criteria and the unassessed description obligation')
    for row in rows:
        if facts.get(row['testId']) != (row['name'], row.get('method')):
            raise ValueError('Stale Fact name, trait or method reference')
        if scenarios.get(row['name']) != row.get('scenarioFile'):
            raise ValueError('Missing or stale source scenario reference')
        section = re.search(rf'^## {row["acceptanceId"]} — .*?(?=^## |\Z)', requirements, re.M | re.S).group()
        for reference in [row['name'], row['testId'], row['method'], row['scenarioFile']]:
            if f'`{reference}`' not in section:
                raise ValueError('Acceptance section disagrees with declared mapping')

    sources = [{'file': file, 'sha256': hashlib.sha256(read_source(root, file)).hexdigest()} for file in sorted(files)]
    mapping_digest = hashlib.sha256(json.dumps(sources, sort_keys=True, separators=(',', ':')).encode()).hexdigest()
    canonical = {'schemaVersion': 1, 'source': {'system': 'guido-agentic-pipeline/declared-acceptance-map', 'revision': commit},
                 'tests': [{'id': row['acceptanceId'], 'key': row['name'], 'execution': 'expected',
                            'pipelineTestId': row['testId']} for row in rows],
                 'mapping': {'kind': 'declared-links-not-behavioural-proof', 'sourceDigest': mapping_digest, 'files': sources}}
    coverage = {'schemaVersion': 1, 'commit': commit, 'sourceDigest': mapping_digest,
                'scope': 'known-local-acceptance-obligations-not-exhaustive-product-coverage',
                'declaredCriteria': len(headings), 'linkedCriteria': len(rows),
                'unassessedCriteria': sorted(KNOWN_UNASSESSED),
                'behaviourallyVerified': None,
                'diagnostics': ['STATIC_LINKS_ARE_NOT_EXECUTION', 'DESCRIPTION_OBLIGATION_UNASSESSED']}
    return canonical, coverage


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--commit', required=True)
    parser.add_argument('--output', type=Path, required=True)
    parser.add_argument('--coverage', type=Path, required=True)
    args = parser.parse_args()
    canonical, coverage = build_mapping(Path('.'), args.commit)
    for file, value in [(args.output, canonical), (args.coverage, coverage)]:
        file.parent.mkdir(parents=True, exist_ok=True)
        # Never replace an earlier frozen artifact, including symlinks.
        with file.open('x', encoding='utf-8') as stream:
            json.dump(value, stream, indent=2)
            stream.write('\n')
    print(json.dumps({'linkedCriteria': coverage['linkedCriteria'], 'unassessedCriteria': coverage['unassessedCriteria'],
                      'sourceDigest': coverage['sourceDigest']}))


if __name__ == '__main__':
    main()
