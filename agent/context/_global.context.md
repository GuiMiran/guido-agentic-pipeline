# Current Pipeline context

Owner-approved stack: .NET 8, direct xUnit Facts, Selenium, FluentAssertions,
existing Page Objects and original native TRX evidence. No BDD engine or Allure
requirement. Read `CLAUDE.md` and `docs/direct-xunit.md` for the current contract.

Specs remain versioned reference material in `specs/`. Do not execute Gherkin or
regenerate step bindings. A stable Spec trait links every Fact to its documented
scenario; the independent inventory gate rejects lost, duplicate or extra tests.
Use existing documented locators, explicit waits and fresh drivers per test.
Never weaken assertions, introduce sleeps or infer organizational maturity.

Agent prompts describing older stacks are legacy guidance only. Failure triage
is bounded, opt-in and issue-only. Source changes require a separately reviewed
branch/PR and fresh verification; no automatic merge or deployment.
