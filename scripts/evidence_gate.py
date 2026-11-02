"""Fail-closed inventory from versioned Gherkin, independent of observed results."""
import argparse
import json
import os
from pathlib import Path
import re
from xml.etree import ElementTree as ET


def expected(root):
    names = []
    for file in sorted((root / 'specs').rglob('*.feature')):
        text = file.read_text()
        if re.search(r'^\s*(Scenario Outline|Examples):', text, re.M):
            raise ValueError('Outline inventory needs an explicitly reviewed expansion')
        names.extend(re.findall(r'^\s*Scenario:\s*(.+?)\s*$', text, re.M))
    if not names or len(set(names)) != len(names):
        raise ValueError('Empty or duplicate expected inventory')
    return names


def validate(names, allure, trx):
    if not names or len(set(names)) != len(names):
        raise ValueError('Invalid expected inventory')
    for source, rows in [('allure', allure), ('trx', trx)]:
        observed = [row['name'] for row in rows]
        if len(set(observed)) != len(observed) or set(observed) != set(names):
            raise ValueError(source + ': missing, duplicate or unexpected results')
        if any(row['status'] != 'passed' for row in rows):
            raise ValueError(source + ': non-passing results')


def main():
    p = argparse.ArgumentParser()
    p.add_argument('mode', choices=['inventory', 'check'])
    p.add_argument('--root', type=Path, default=Path('.'))
    p.add_argument('--inventory', type=Path, required=True)
    p.add_argument('--allure', type=Path)
    p.add_argument('--trx', type=Path)
    a = p.parse_args()
    if a.mode == 'inventory':
        doc = {'schemaVersion': 1, 'commit': os.environ.get('GITHUB_SHA'), 'source': 'versioned-specs', 'tests': expected(a.root)}
        a.inventory.parent.mkdir(parents=True, exist_ok=True)
        a.inventory.write_text(json.dumps(doc, indent=2))
        return
    doc = json.loads(a.inventory.read_text())
    if not doc.get('commit') or doc['commit'] != os.environ.get('GITHUB_SHA'):
        raise ValueError('Inventory commit mismatch')
    allure = []
    for file in sorted(a.allure.glob('*-result.json')):
        result = json.loads(file.read_text())
        allure.append({'name': result['name'], 'status': result['status']})
    trx = []
    ns = {'t': 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}
    for file in sorted(a.trx.rglob('*.trx')):
        for row in ET.parse(file).findall('.//t:UnitTestResult', ns):
            trx.append({'name': row.attrib['testName'], 'status': 'passed' if row.attrib['outcome'] == 'Passed' else 'failed'})
    validate(doc['tests'], allure, trx)
    print(f"Verified {len(doc['tests'])} expected scenarios in both Allure and TRX")


if __name__ == '__main__':
    main()
