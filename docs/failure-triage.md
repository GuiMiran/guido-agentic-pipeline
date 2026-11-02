# Failure triage, not autonomous repair

The former auto-heal workflow is replaced by an issue-only boundary. It is off
unless the owner explicitly sets `GUIDO_FAILURE_TRIAGE_ENABLED=true` in repository
variables after reviewing the workflow. This change does not set that variable.
No model, code repair, checkout, artifact execution, branch write or deployment
is performed by the privileged job. Existing GENERATE/HEAL prompts are reference
material, never executable authority.

Only failed `Run Tests` pushes on the default branch of this repository are
eligible. Fork/PR events and other branches are rejected. Manual dispatch must
also use the default branch. Inputs are read from the event object as bounded
data, indented in an issue, and never interpolated into shell or JavaScript.
Run identifiers are validated and repeated events are deduplicated by marker.
No raw artifact is downloaded or executed. Failure is not assumed to be a test
defect: humans must distinguish build, infrastructure and behavioural failures.

The workflow has read-only default permissions and only issues-write authority
on the triage job. It uses a commit-pinned official action and a timeout. Review
the dedicated security-control CI, including malicious multiline inputs and
provenance rejection. These tests use a fake issue API and send no real issues.

The existing cart timeout and absent raw Allure evidence remain separate work.
Do not interpret this workflow or a green security-control job as E2E success.
