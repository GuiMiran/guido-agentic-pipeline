import sys
import unittest
from pathlib import Path
sys.path.insert(0, str(Path(__file__).resolve().parents[1] / 'scripts'))
from evidence_gate import validate


class EvidenceGateTests(unittest.TestCase):
    def test_complete(self):
        rows = [{'name': 'one', 'status': 'passed'}]
        validate(['one'], rows, rows)

    def test_empty_missing_duplicate_unexpected_and_nonpass(self):
        valid = [{'name': 'one', 'status': 'passed'}]
        for invalid in [[], valid * 2, [{'name': 'other', 'status': 'passed'}],
                        [{'name': 'one', 'status': status} for status in ['failed']],
                        [{'name': 'one', 'status': 'broken'}], [{'name': 'one', 'status': 'skipped'}]]:
            for first, second in [(invalid, valid), (valid, invalid)]:
                with self.subTest(rows=invalid):
                    with self.assertRaises(ValueError):
                        validate(['one'], first, second)

    def test_empty_inventory(self):
        with self.assertRaises(ValueError):
            validate([], [], [])
