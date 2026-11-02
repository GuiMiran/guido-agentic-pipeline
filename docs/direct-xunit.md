# Direct xUnit execution

This change preserves the 23 named scenarios (5 Auth, 7 Inventory, 5 Cart,
6 Checkout), their assertions, documented selectors and the merged cart fix.
Each scenario is now one xUnit Fact with its original DisplayName and stable
Spec trait. `specs/` remains human-readable specification; it is not executed
by a BDD engine. Generated features, bindings and lifecycle hooks are removed.
BaseTest creates/disposes a driver for every test instance. No shared fixture
or ignored test is introduced. Versioned data backs invalid login/checkout.

CI freezes the versioned scenario inventory and compares it with the authored
Fact names/Spec traits before execution. It then requires exactly those original
names once each and Passed outcomes in the retained native TRX. A readable
Markdown summary is presentation, never the evidence source. Allure and
Reqnroll are not dependencies. Original failed evidence is preserved.

The local test-quality rubric is separate from organizational GUIDO maturity;
no automated GUIDO score/check or branch-protection enforcement is claimed.
The bounded issue-only triage remains disabled unless its owner enables it.

CI also freezes [declared SPECTRA acceptance links](spectra-acceptance-mapping.md)
as a separate canonical inventory. Its ledger retains five known unassessed
obligations; a passing 23-test run does not certify those rules.

Run `dotnet test src/GUIDO.Agentic.Tests/GUIDO.Agentic.Tests.csproj --configuration Release`.
The migration must pass all 23 cases in fresh CI before review/merge. Source
inspection or the old generated tests passing is not migration evidence.
