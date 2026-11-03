# Declared SPECTRA acceptance links

## Scope and review boundary

The execution stack remains .NET 8, direct xUnit, Selenium and native TRX.
No BDD engine, Allure dependency or undocumented selector is introduced.
Original assertions remain unchanged; dedicated Facts add stronger checks.

[Layer 11](../.spectra/11-acceptance-criteria.md) preserves AC-001 through AC-023
and adds dedicated test links for AC-024, AC-026, AC-027 and AC-028. The suite
now contains 27 scenarios; no original test or assertion was removed.
[spectra-mapping.json](../specs/spectra-mapping.json) retains their original
domain IDs, exact DisplayName, scenario file and C# method references.
This is a proposed semantic mapping for **owner PR review**, not an agent's
self-approved oracle. Merge ratifies declarations, not behavioural correctness.

Existing requirements are not weakened to fit the tests. Follow-up state:

| ID | Documented obligation | Existing gap |
| --- | --- | --- |
| AC-024 | Exactly 6 products for standard_user | Dedicated exact-count Fact; requires fresh passing evidence |
| AC-025 | Each product displays a description | UNASSESSED: no documented selector; owner authorization required before verifying/documenting one |
| AC-026 | Cart-page removal also clears the badge | Dedicated cart-page list+badge Fact; requires fresh passing evidence |
| AC-027 | Unauthenticated inventory access is denied | Fresh-session direct-access Fact; requires fresh passing evidence |
| AC-028 | Overview displays the added product's identity | Name+price captured before add and matched in the single overview row; requires fresh passing evidence |

28 IDs are a known local inventory, **not an exhaustive product denominator**.
No coverage percentage or GUIDO score is inferred. Other untested rules may
exist. Passing 27 linked scenarios does not mean the whole product passes.

## Freeze before execution

```sh
python3 -m unittest discover -s test -p 'test_*.py' -v
python3 scripts/spectra_mapping.py --commit FULL_COMMIT_SHA \
  --output /tmp/run-evidence/spectra-expected.json \
  --coverage /tmp/run-evidence/acceptance-links.json
```

The validator requires a bijection between the independent 27-scenario
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
- `spectra-expected.json`: canonical SPECTRA inventory for 27 linked scenarios,
  retaining original Pipeline ID as metadata.
- `acceptance-links.json`: structural scope ledger including unassessed AC-025
  and `behaviourallyVerified: null`.
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
producing UNMAPPED relationships. Missing, non-passing, duplicate and
unexpected results still fail closed. The importer does not consume the separate
structural ledger or certify the unassessed description obligation. Preserve that
ledger with the snapshot; zero measured test gaps is not full product coverage.

`ingest-trx` does not create `.spectra/evidence.json`. A default `spectra verify`
against layer 11 still requires evidence for **all 28** criteria. Do not silently
narrow the acceptance gate to make it green.

## Follow-up backlog

| Work | State |
| --- | --- |
| Review and ratify scenario-to-AC declarations | This PR |
| Fresh native CI and host-assisted SPECTRA import | Required before ready for review |
| Tests/assertions for AC-024, AC-026, AC-027, AC-028 | This change; require fresh native CI |
| Verify and document description selector, then test AC-025 | BLOCKED: owner authorization under locator contract |
| Authenticated automatic Pipeline → SPECTRA handoff | Pending; currently host-assisted |

No full A1/A2, automatic repair, organizational score, model-backed evaluation
or autonomous promotion is claimed.
