"""Referential-integrity controls; no static check is a behavioural oracle."""
import copy
import json
from pathlib import Path
import shutil
import subprocess
import sys
import tempfile
import unittest

sys.path.insert(0, str(Path(__file__).resolve().parents[1] / 'scripts'))
from spectra_mapping import build_mapping, MANIFEST, REQUIREMENTS, TESTS

SOURCE = Path(__file__).resolve().parents[1]
COMMIT = 'a' * 40


class MappingTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory(prefix='pipeline-mapping-')
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        for relative in ['specs', '.spectra', 'src/GUIDO.Agentic.Tests/Tests', 'src/GUIDO.Agentic.Tests/Pages']:
            shutil.copytree(SOURCE / relative, self.root / relative)

    def manifest(self):
        return json.loads((self.root / MANIFEST).read_text())

    def write_manifest(self, manifest):
        (self.root / MANIFEST).write_text(json.dumps(manifest))

    def test_complete_declared_links_keep_unassessed_obligations_separate(self):
        expected, coverage = build_mapping(self.root, COMMIT)
        self.assertEqual(len(expected['tests']), 23)
        self.assertEqual(expected['tests'][0]['id'], 'AC-001')
        self.assertEqual(expected['tests'][0]['pipelineTestId'], 'AUTH-001')
        self.assertEqual(coverage['declaredCriteria'], 28)
        self.assertEqual(coverage['linkedCriteria'], 23)
        self.assertEqual(coverage['unassessedCriteria'], [f'AC-{i:03}' for i in range(24, 29)])
        self.assertIsNone(coverage['behaviourallyVerified'])
        self.assertEqual((expected, coverage), build_mapping(self.root, COMMIT))
        self.assertTrue(any(item['file'] == REQUIREMENTS for item in expected['mapping']['files']))

    def test_deleted_or_duplicate_mappings_are_rejected(self):
        original = self.manifest()
        for rows in [original['mappings'][:-1], original['mappings'] + [original['mappings'][0]],
                     [original['mappings'][0]] * 23]:
            with self.subTest(rows=len(rows)):
                modified = copy.deepcopy(original)
                modified['mappings'] = rows
                self.write_manifest(modified)
                with self.assertRaises(ValueError):
                    build_mapping(self.root, COMMIT)

    def test_stale_names_ids_methods_and_scenario_paths_are_rejected(self):
        original = self.manifest()
        for field, value in [('testId', 'AUTH-999'), ('name', 'Renamed test'), ('acceptanceId', 'AC-999'),
                             ('method', 'AuthTests.Missing'), ('scenarioFile', 'specs/missing.feature'),
                             ('scenarioFile', '../outside.feature')]:
            with self.subTest(field=field):
                changed = copy.deepcopy(original)
                changed['mappings'][0][field] = value
                self.write_manifest(changed)
                with self.assertRaises(ValueError):
                    build_mapping(self.root, COMMIT)

    def test_broken_acceptance_backlink_is_rejected(self):
        file = self.root / REQUIREMENTS
        file.write_text(file.read_text().replace('`AuthTests.StandardLogin`', '`AuthTests.Unknown`'))
        with self.assertRaisesRegex(ValueError, 'disagrees'):
            build_mapping(self.root, COMMIT)

    def test_removed_or_duplicate_criteria_and_hidden_unassessed_scope_are_rejected(self):
        file = self.root / REQUIREMENTS
        original = file.read_text()
        for changed in [original.replace('## AC-024 —', '## AC-999 —'), original + '\n## AC-001 — Duplicate\n',
                        original.replace('Status: UNASSESSED', 'Status: COVERED')]:
            file.write_text(changed)
            with self.assertRaises(ValueError):
                build_mapping(self.root, COMMIT)

    def test_stale_fact_or_documented_scenario_is_rejected(self):
        for relative, before, after in [(TESTS, 'void StandardLogin()', 'void RenamedLogin()'),
                                        ('specs/auth/login.feature', 'Scenario: Successful login', 'Scenario: Renamed login')]:
            file = self.root / relative
            original = file.read_text()
            file.write_text(original.replace(before, after))
            with self.assertRaises(ValueError):
                build_mapping(self.root, COMMIT)
            file.write_text(original)

    def test_duplicate_criterion_assignment_is_rejected(self):
        changed = self.manifest()
        changed['mappings'][1]['acceptanceId'] = changed['mappings'][0]['acceptanceId']
        self.write_manifest(changed)
        with self.assertRaises(ValueError):
            build_mapping(self.root, COMMIT)

    def test_helper_bytes_change_digest_without_becoming_proof(self):
        _, before = build_mapping(self.root, COMMIT)
        helper = self.root / 'src/GUIDO.Agentic.Tests/Pages/InventoryPage.cs'
        helper.write_text(helper.read_text() + '\n// changed dependency\n')
        _, after = build_mapping(self.root, COMMIT)
        self.assertNotEqual(before['sourceDigest'], after['sourceDigest'])
        self.assertIsNone(after['behaviourallyVerified'])

    def test_rejects_symlink_sources_and_invalid_commit(self):
        file = self.root / REQUIREMENTS
        target = self.root / 'external.md'
        file.rename(target)
        file.symlink_to(target)
        with self.assertRaisesRegex(ValueError, 'Symlink'):
            build_mapping(self.root, COMMIT)
        for commit in [None, '', 'abcdef0', 'not-a-commit']:
            with self.assertRaises(ValueError):
                build_mapping(self.root, commit)

    def test_cli_freezes_once_and_does_not_replace_existing_artifacts(self):
        output, coverage = self.root / 'expected.json', self.root / 'links.json'
        args = [sys.executable, str(SOURCE / 'scripts/spectra_mapping.py'), '--commit', COMMIT,
                '--output', str(output), '--coverage', str(coverage)]
        first = subprocess.run(args, cwd=self.root, capture_output=True, text=True)
        self.assertEqual(first.returncode, 0, first.stderr)
        original = output.read_bytes()
        second = subprocess.run(args, cwd=self.root, capture_output=True, text=True)
        self.assertNotEqual(second.returncode, 0)
        self.assertEqual(original, output.read_bytes())


if __name__ == '__main__':
    unittest.main()
