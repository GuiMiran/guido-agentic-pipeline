# Decision 0001 — Read-only authenticated evidence transfer

- Status: Proposed; owner review required
- Date: 2026-11-03

Use Pipeline's GitHub-hosted CI to authenticate source metadata, archive hashes
and versioned expected inventories with an ephemeral Actions-read token. A
separate job uses a fixed SPECTRA commit to import bounded TRX data with no
acquisition token in its child environment. Artifacts never supply code paths,
commands, checkout refs, evaluators, credentials or workflow instructions.

Rationale: `workflow_run` can carry privileged authority, so avoid source/PR
checkout and explicitly restrict every job to reads. GitHub's automatic digest
validation can emit a warning rather than fail; require an explicit hard-fail
archive hash check. The expected inventory is anchored to authenticated source
Git blobs, not reconstructed from the observed test result.

Results remain Pipeline artifacts: no PAT, repository dispatch, SPECTRA commits,
issue creation, branch protection change, Docker, provider call or source repair.
This deliberately implements cross-repository **tool consumption**, not remote
mutation. Independent collector/attestation trust and product coverage remain
separate. The same-repo PR path is an explicitly labeled read-only smoke, while
automatic production `workflow_run` requires merge into the default branch.

References:
- [GitHub workflow_run semantics and security warning](https://docs.github.com/en/actions/reference/workflows-and-actions/events-that-trigger-workflows#workflow_run)
- [Secure use reference](https://docs.github.com/en/actions/reference/security/secure-use)
- [Artifacts API and Actions-read scope](https://docs.github.com/en/rest/actions/artifacts)
- [Artifact digest validation behaviour](https://docs.github.com/en/actions/tutorials/store-and-share-data)
