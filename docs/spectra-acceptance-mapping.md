# Declared SPECTRA acceptance links

## Scope and review boundary

The execution stack remains .NET 8, direct xUnit, Selenium and native TRX.
No BDD engine, Allure dependency, locator or assertion change is introduced.

[Layer 11](../.spectra/11-acceptance-criteria.md) assigns AC-001 through AC-023
to existing scenarios, using actual assertion scope.
[spectra-mapping.json](../specs/spectra-mapping.json) retains their original
domain IDs, exact DisplayName, scenario file and C# method references.
This is a proposed semantic mapping for **owner PR review**, not an agent's
self-approved oracle. Merge ratifies declarations, not behavioural correctness.

Existing requirements are not weakened to fit the tests. Five known wider
obligations remain explicitly UNASSESSED:

| ID | Documented obligation | Existing gap |
| --- | --- | --- |
| AC-024 | Exactly 6 products for standard_user | Products() only asserts count > 0 |
| AC-025 | Each product displays a description | Field helper omits description |
| AC-026 | Cart-page removal also clears the badge | Test asserts only the empty list |
| AC-027 | Unauthenticated inventory access is denied | Fixtures always log in |
| AC-028 | Overview displays the added product's identity | Helper checks only that some item exists |

28 IDs are a known local inventory, **not an exhaustive product denominator**.
No coverage percentage or GUIDO score is inferred. Other untested rules may
exist. Passing 23 linked scenarios does not mean the whole product passes.

## Freeze before execution

```sh
python3 -m unittest discover -s test -p 'test_*.py' -v
python3 scripts/spectra_mapping.py --commit FULL_COMMIT_SHA \
  --output /tmp/run-evidence/spectra-expected.json \
  --coverage /tmp/run-evidence/acceptance-links.json
```

The validator requires a bijection between the independent 23-scenario
inventory, authored Facts, stable Spec traits and declared AC links. It checks
definitions, reverse references, scenario file membership and method names.
Missing, duplicate, stale, unknown and unsafe references fail. The scanner
supports this repository's current Fact layout, not arbitrary C#; layout or
scope changes require review. It cannot assess assertion semantics.

Source hashes cover the mapping, criteria, feature/context files and authored
C# dependencies. A changed helper changes the digest, never a behavioural
verdict. Hashes bind bytes, not truth; the full commit binds the revision.
CI freezes outputs before restore/build/test and retains them beside TRX:

- `expected.json`: unchanged domain-ID inventory used by the native TRX gate.
- `spectra-expected.json`: canonical SPECTRA inventory for 23 linked scenarios,
  retaining original Pipeline ID as metadata.
- `acceptance-links.json`: structural scope ledger including five unassessed
  criteria and `behaviourallyVerified: null`.
- `trx/tests.trx`: original independently executed outcomes.

Output creation refuses existing files, including symlinks. No test result is
inferred while freezing links.

## Consume with SPECTRA

After fresh CI, verify artifact custody and the executed commit. Use SPECTRA
with [PR #22](https://github.com/GuiMiran/spectra/pull/22) included:

```sh
spectra ingest-trx --input ./downloaded/trx \
  --expected ./downloaded/spectra-expected.json \
  --run-id pipeline-RUN_ID-ATTEMPT --commit EXECUTED_FULL_COMMIT_SHA --json
```

The executed revision is `spectra-expected.json` → `source.revision`, normally
GitHub's synthetic merge commit for PR events, not an older green commit or an
unrelated branch head. The generic canonical inventory relies on trusted host
custody: the host must verify this binding. SPECTRA does not authenticate GitHub.

For this scoped inventory, recorded outcomes can link to AC IDs instead of
producing 23 UNMAPPED relationships. Missing, non-passing, duplicate and
unexpected results still fail closed. The importer does not consume the separate
structural ledger or certify the five unassessed obligations. Preserve that
ledger with the snapshot; zero measured test gaps is not full product coverage.

`ingest-trx` does not create `.spectra/evidence.json`. A default `spectra verify`
against layer 11 still requires evidence for **all 28** criteria. Do not silently
narrow the acceptance gate to make it green.

## Follow-up backlog

| Work | State |
| --- | --- |
| Review and ratify scenario-to-AC declarations | This PR |
| Fresh native CI and host-assisted SPECTRA import | Required before ready for review |
| Tests/assertions for AC-024 through AC-028 | Pending; separate behavioural changes |
| Authenticated automatic Pipeline → SPECTRA handoff | Pending; currently host-assisted |

No full A1/A2, automatic repair, organizational score, model-backed evaluation
or autonomous promotion is claimed.
