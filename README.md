# GUIDO Agentic Pipeline

[Agent entry](AGENTS.md) · [Shared GUIDO SDD governance hub](https://github.com/GuiMiran/spectra/tree/main/docs/ecosystem)
for component ownership, version baselines and cross-repository traceability.

C#/.NET 8 + direct xUnit + Selenium Page Objects. No SpecFlow, Reqnroll or
mandatory Allure. Specs drive review and traceability; Gherkin is documentation,
not an execution dependency. The 23 original scenario names and assertions are
preserved and each Fact has a stable Spec trait. Five obligation-focused Facts
extend the current suite to 28 scenarios without replacing the original 23.

Run `dotnet test src/GUIDO.Agentic.Tests/GUIDO.Agentic.Tests.csproj --configuration Release`.
Read [the execution contract](docs/direct-xunit.md) and [failure triage policy](docs/failure-triage.md).
CI freezes an independent scenario inventory before execution and requires a
complete passing native TRX, retaining original files, commit and file digests.
A Markdown summary is a view of evidence, not a replacement for it.

[SPECTRA acceptance mapping](docs/spectra-acceptance-mapping.md) declares links
from those 28 scenarios to AC IDs. AC-025 now has an owner-authorized,
documented description selector and a dedicated presence/visibility test.
CI freezes the canonical inventory and separate structural ledger before tests.
Static links and a green execution are not full functional coverage.

[Authenticated evidence handoff](docs/evidence-handoff.md) transfers verified
native artifacts to a pinned SPECTRA importer in a separate read-only CI job.
Results stay as CI artifacts; no PAT, SPECTRA source push or auto-repair is used.

Current modules: Auth (6), Inventory (9), Cart (6), Checkout (7), against the
public SauceDemo demonstration application. This is not exhaustive coverage.

Failure triage remains opt-in and issue-only; no automatic repair, merge or
deployment is enabled. Static agent prompts are guidance, not execution powers.
Legacy SpecFlow/Allure examples in historical architecture or prompt documents
do not define the active stack. No GUIDO organizational score or enforced score
check is claimed. See [the local quality rubric](docs/GUIDO-SCALE.md) separately.

Author: Guido Miranda Mercado — [GitHub](https://github.com/GuiMiran).
