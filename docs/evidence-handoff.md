# Authenticated Pipeline → SPECTRA evidence handoff

## What runs automatically

After this workflow is merged into `main`, completion of `Run Tests` on a
same-repository `main` **push** starts `SPECTRA evidence handoff`. It consumes
successful or failed completed runs; PRs, forks, cancellations and other
workflows are not production sources. No variable, PAT or new secret is needed.

The authenticated transfer runs SPECTRA's pinned importer on CI and publishes
its result as a **Pipeline workflow artifact**. It does not push files into the
SPECTRA repository or change either repository's source. This is an evidence
adapter, not cross-repository source execution, repair or autonomous promotion.

| Job | Authority | Work |
| --- | --- | --- |
| controls | contents:read | Deterministic adversarial Node/Python tests |
| acquire | contents:read, actions:read | Authenticate run/artifact/source Git blobs via GitHub API; transfer inert ZIP + custody record |
| consume | contents:read only | Revalidate ZIP/inventory; run pinned SPECTRA and audit the authenticated source checkout as data, without acquisition token |

No job has write permissions, OIDC, referenced secrets or cached untrusted
dependencies. Upload/download of same-workflow artifacts uses GitHub's runtime
artifact service; that service credential is not passed to the importer child.
SPECTRA checkout is fixed to
`025df70f7be2fc024e451eda9033909b691e9209`. Updating it requires review.
No npm installation or package lifecycle hook runs.

## Provenance and inventory checks

The allow-list fixes repository ID `1190006301`, workflow ID `250433858`, path
`.github/workflows/run-tests.yml`, name `Run Tests`, event `push`, branch `main`,
head repository and full source SHA. Event/API attempt or commit drift fails.
Artifact selection requires exactly `pipeline-trx-<run-id>-<attempt>`, one match,
unexpired metadata, bounded size and matching run/repository/head identities.
The original ZIP must match GitHub API SHA-256 and byte length; a digest warning
from an action alone is not treated as verification.

The acquisition job reads the source tree at the **executed source commit** via
GitHub API. It verifies Git blob IDs and computes the same SHA-256 dependency
index as the producer, without checking out or executing producer code. The
canonical mapping and acceptance definitions are read as bounded UTF-8 data.
The consumer checks artifact inventories against that versioned mapping, and
their source hash index against authenticated blobs. Inventory, scope ledger
and native summary must all bind to the same source commit.

The ZIP is data only. Exactly five files are allowed: `expected.json`,
`spectra-expected.json`, `acceptance-links.json`, `summary.json`, `trx/tests.trx`.
Missing/extra/duplicate entries, traversal, symlinks, encrypted entries,
unsupported compression, duplicate JSON keys and oversized content fail before
extraction. Limits: ZIP 8 MiB, expanded total 32 MiB, TRX 16 MiB, JSON 2 MiB,
at most 100 source dependencies of 2 MiB each. No archive entry is executable.

The consumer checks the actual SPECTRA Git HEAD, then starts its importer with
a clean child environment containing PATH only. The native original TRX—not a
rendered summary—determines test qualification. A failed source workflow,
unknown qualification or remaining unassessed criteria cannot yield overall
PASS. Malformed data fails the workflow; well-formed negative evidence retains
its snapshot, gaps and non-passing receipt.

## Retained evidence

- `verified-handoff-input`: original ZIP and custody record from acquisition.
- `spectra-evidence-<consumer-run>-<attempt>`: `.spectra/evidence/test-runs/`,
  coverage map, receipt, custody and acceptance scope ledger, retained 30 days.
- `receipt.json`: source run/attempt/commit, artifact ID/digest, SPECTRA pin,
  actual parsed counts, qualification, unassessed criteria and output hashes.
- `a1/`: pinned Scale inventory audit, A1 contracts, gap review and a receipt
  binding their hashes to the authenticated source run. The source checkout
  and auditor pin must match and stay clean. Source code is never executed
  by the consumer. The A1 step retains valid negative evidence without
  making a failed import green; malformed or missing output fails closed.
  Scoped PASS generates no plan, and a measured failure can only generate
  a deterministic human-review proposal. There is no execution or scoring.

Hashes establish byte identity under GitHub/runner trust, not independent
collector authenticity, assertion quality or a cryptographically signed supply
chain attestation. Main source and receiving workflow remain maintainer-trusted;
repository rulesets/CODEOWNERS enforcement are not configured by this change.
No result establishes exhaustive product coverage or a GUIDO G1–G5 score.

## PR verification versus production

Changes to the handoff's files also run a **same-repository PR smoke**. It
authenticates and consumes the latest completed `main` push from the fixed
workflow (including failures), never a PR-supplied run or PR artifact. It proves
the real API/token/download/job-isolation/import path before merge, but is
labeled `mode: pull_request`, not production `workflow_run`.
Forks cannot run acquisition. Unit controls still run without artifact-read
authority. There is no `pull_request_target`, privileged source checkout or
manual run-ID bypass. If the latest source artifact is absent/expired, the
smoke fails closed; do not silently substitute an older green result.

After merge, verify the first automatic `workflow_run` receipt separately.
The PR proof and run links are recorded in the PR body; production activation
cannot be claimed merely from a new workflow file's presence on a branch.

## Backlog

| Scope | State |
| --- | --- |
| Authenticated read-only artifact handoff + pinned SPECTRA consumer | This PR; requires live PR smoke and maintainer review |
| First production workflow_run receipt after merge | Pending merge and next main Run Tests completion |
| Cross-source aggregation / canonical acceptance evidence promotion | Not implemented |
| External collector attestation, private oracle evaluation, general A2 runtime | Separate future work |

See [decision 0001](decisions/0001-authenticated-evidence-handoff.md).
