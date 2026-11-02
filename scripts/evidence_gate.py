"""Independent documented scenario inventory and native TRX gate; no BDD engine."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import re
from xml.etree import ElementTree as ET


def inventory(root):
    names = []
    for file in sorted((root / 'specs').rglob('*.feature')):
        text = file.read_text()
        if re.search(r'^\s*(Scenario Outline|Examples):', text, re.M):
            raise ValueError('Unsupported outline requires reviewed inventory expansion')
        names += re.findall(r'^\s*Scenario:\s*(.+?)\s*$', text, re.M)
    facts = []
    for file in (root / 'src/GUIDO.Agentic.Tests/Tests').glob('*.cs'):
        facts += re.findall(r'\[Fact\(DisplayName = "([^"]+)"\), Trait\("Spec", "([^"]+)"\)\]', file.read_text())
    if len(names) != 23 or len(set(names)) != len(names):
        raise ValueError('The reviewed migration scope must retain all 23 scenarios')
    if len(facts) != len(names) or set(n for n, _ in facts) != set(names) or len({i for _, i in facts}) != len(facts):
        raise ValueError('Missing/duplicate/mismatched Fact names or Spec IDs')
    return [{'id': ident, 'name': name} for name, ident in facts]


def validate(expected, rows):
    if not expected or len(set(expected)) != len(expected):
        raise ValueError('Invalid inventory')
    names = [row['name'] for row in rows]
    if len(names) != len(set(names)) or set(names) != set(expected):
        raise ValueError('Missing, duplicate or unexpected TRX result')
    if any(row['status'] != 'Passed' for row in rows):
        raise ValueError('Non-passing TRX result')


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('mode', choices=['inventory', 'check'])
    parser.add_argument('--inventory', type=Path, required=True)
    parser.add_argument('--trx', type=Path)
    args = parser.parse_args()
    if args.mode == 'inventory':
        doc = {'schemaVersion': 1, 'commit': os.environ.get('GITHUB_SHA'), 'tests': inventory(Path('.'))}
        args.inventory.parent.mkdir(parents=True, exist_ok=True)
        args.inventory.write_text(json.dumps(doc, indent=2))
        return
    doc = json.loads(args.inventory.read_text())
    if not doc.get('commit') or doc['commit'] != os.environ.get('GITHUB_SHA'):
        raise ValueError('Inventory commit mismatch')
    rows, sources = [], []
    for file in sorted(args.trx.rglob('*.trx')):
        sources.append({'file': file.name, 'sha256': hashlib.sha256(file.read_bytes()).hexdigest()})
        for row in ET.parse(file).findall('.//t:UnitTestResult', {'t': 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}):
            rows.append({'name': row.attrib['testName'], 'status': row.attrib['outcome']})
    summary = {'commit': doc['commit'], 'expected': len(doc['tests']), 'observed': len(rows),
               'passed': sum(r['status'] == 'Passed' for r in rows), 'sources': sources}
    (args.inventory.parent / 'summary.json').write_text(json.dumps(summary, indent=2))
    if os.environ.get('GITHUB_STEP_SUMMARY'):
        with open(os.environ['GITHUB_STEP_SUMMARY'], 'a') as out:
            out.write(f"## Native TRX evidence\nExpected: {summary['expected']}; observed: {len(rows)}; passed: {summary['passed']}.\n")
    validate([item['name'] for item in doc['tests']], rows)
    print(json.dumps(summary))


if __name__ == '__main__':
    main()
