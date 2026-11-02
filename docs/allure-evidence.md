# Native Allure evidence gate

Allure.SpecFlow 2.15.0 instruments the existing SpecFlow 3.9 scenarios via its
documented step assembly. See https://allurereport.org/docs/specflow/ and
https://www.nuget.org/packages/Allure.SpecFlow/2.15.0 . No runner migration is
included; migration to Reqnroll remains a separately reviewed decision.

CI freezes scenario names from versioned specs before test execution, binding
the inventory to the tested commit. Outlines and duplicate names fail closed
until an explicit expansion policy exists. Allure and TRX must each contain
exactly the expected names and passing results. Empty, missing, duplicate,
unexpected, failed, broken and skipped results cannot pass. These are scenario
identities, not canonical SPECTRA acceptance IDs or an organizational score.

The build and tests use the same Release configuration. Native evidence and the
inventory are written to a fresh runner temporary directory and retained even
on test failure. Tests are never marked continue-on-error. The old unpinned
HTML renderer is removed; raw results are the required evidence. HTML may be
generated separately from the retained data, but is not a substitute for it.

This PR does not fix the known cart interaction; review/merge its separate PR
first or expect this gate to remain red on that failure. No branch protection,
privileged triage enablement or external model is configured automatically.
