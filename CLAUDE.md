# Pipeline agent contract — direct C# tests

The owner approved replacing SpecFlow with direct xUnit tests, without Reqnroll
or mandatory Allure. Keep Selenium, existing Page Objects and .NET 8.

- Author tests in `src/GUIDO.Agentic.Tests/Tests/` using xUnit Facts.
- Each fact needs a stable Spec trait and a backing scenario in versioned specs.
- `specs/**/*.feature` is documentation, not executable Gherkin. No step bindings,
  generated feature code, BDD runtime or speculative package migration.
- Never invent locators: use those in the module context and Page Objects.
- No Thread.Sleep, weakened assertions, silent skips or hidden retries.
- Read URLs and credentials through ConfigManager/versioned test data.
- One fresh driver per test; deterministic disposal, no shared test state.
- Run dotnet test in Release; retain original TRX. Empty, duplicate, missing,
  unexpected or non-passing results must not pass the evidence gate.
- Failure triage is opt-in and issue-only, never authority to modify source.
- No GUIDO maturity score is inferred from a green test run.

Historical prompts/docs describing SpecFlow are legacy reference only; this
owner-approved contract and `docs/direct-xunit.md` define current execution.
