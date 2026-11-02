# Cart multi-add candidate fix

The recorded failure is the two-product cart scenario. Its multi-add binding
used native clicks and duplicated page locators, unlike the passing single-add
page method's React click path. Move multi-add to InventoryPage, use only the
existing documented locators and click convention, wait for inventory load,
require an initially empty cart, and re-resolve the badge after each addition.
The existing assertion still requires exactly two cart items. No sleeps,
increased timeout, skipped tests or changed expectation are introduced.

This is a proposed fix, not proof of root cause. Acceptance requires a fresh
focused scenario and the complete 23-scenario suite in CI, retaining TRX.
The security PR and absent Allure instrumentation remain separate work.
