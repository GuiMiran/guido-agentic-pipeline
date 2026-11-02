import sys
import unittest
from pathlib import Path
sys.path.insert(0, str(Path(__file__).resolve().parents[1] / 'scripts'))
from evidence_gate import validate


class GateTests(unittest.TestCase):
    def test_passing(self):
        validate(['one'], [{'name': 'one', 'status': 'Passed'}])

    def test_bad_results(self):
        passing = {'name': 'one', 'status': 'Passed'}
        variants = [[], [passing, passing], [{'name': 'other', 'status': 'Passed'}]]
        variants += [[{'name': 'one', 'status': state}] for state in ['Failed', 'NotExecuted', 'Unknown', 'Timeout']]
        for rows in variants:
            with self.subTest(rows=rows), self.assertRaises(ValueError):
                validate(['one'], rows)

    def test_empty_inventory(self):
        with self.assertRaises(ValueError):
            validate([], [])
