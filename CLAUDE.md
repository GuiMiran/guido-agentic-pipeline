# GUIDO Agentic Pipeline — Claude Context

## Project
Self-maintaining agentic E2E test pipeline for SauceDemo.
Target URL: https://www.saucedemo.com

## Stack
- Language: C# (.NET 8)
- BDD: SpecFlow 3.9
- Runner: xUnit
- UI Automation: Selenium WebDriver 4.x
- Driver Mgmt: WebDriverManager (no manual chromedriver)
- Assertions: FluentAssertions
- Reporting: Allure
- Pattern: Page Component Model

## Hard Rules
- NEVER use `Thread.Sleep` — only `WebDriverWait` / explicit waits via `BasePage` helpers
- NEVER hardcode URLs or credentials — always use `ConfigManager`
- NEVER generate code without a `.feature` file backing it
- ALL locators must be declared in the feature's `.context.md`
- If a locator is not in context → **ASK**, do not invent

## Known Patterns (learned from SauceDemo — apply to any site)

### React-controlled inputs
`SendKeys` alone does NOT update React's internal state.
Always use the native value setter pattern:
```csharp
_js.ExecuteScript(
    "var s = Object.getOwnPropertyDescriptor(window.HTMLInputElement.prototype, 'value').set;" +
    "s.call(arguments[0], arguments[1]);" +
    "arguments[0].dispatchEvent(new Event('input', {bubbles:true}));",
    field, value);
```
**Symptoms if missing:** form validation ignores typed values ("Field X is required" even when filled).
**Detection:** if `SendKeys` works on login but not on other forms → React state mismatch.

### Clicks on React buttons
Prefer JS click over native `.Click()` for React-rendered buttons in SPAs:
```csharp
_js.ExecuteScript("arguments[0].click();", WaitFor("[data-test='btn']"));
```
Native `.Click()` can be intercepted by React's synthetic event system.

### Explicit wait before every interaction
Never trust that elements are interactable after navigation.
Always resolve elements via `_wait.Until(d => d.FindElement(...))` — never `_driver.FindElement` directly in Page Objects.
```csharp
private IWebElement WaitFor(string css) =>
    _wait.Until(d => d.FindElement(By.CssSelector(css)));
```

### Wait after state-changing actions
After any action that mutates DOM state (remove item, close modal, badge update):
wait for the RESULT, not just after the click:
```csharp
int countBefore = _driver.FindElements(By.CssSelector(".badge")).Count;
btn.Click();
_wait.Until(d => d.FindElements(By.CssSelector(".badge")).Count < countBefore);
```

### SpecFlow Background keyword alignment
`And` inherits the keyword type of the previous step.
If a step is defined as `[When]`, it must be reached via `When` or `And` after `When` — not via `And` after `Given`.
**Pattern for mixed Given/When backgrounds:**
```gherkin
Background:
  Given I am logged in as standard user      # [Given]
  And I add first product to cart            # [Given] ✓
  When I navigate to the cart page           # [When] — resets context
  And I click checkout                       # [When] ✓ (inherits)
```

## File Conventions
- Pages → `src/GUIDO.Agentic.Tests/Pages/<Module>Page.cs`
- Steps → `src/GUIDO.Agentic.Tests/StepDefinitions/<Module>Steps.cs`
- Features → `src/GUIDO.Agentic.Tests/Features/<Module>.feature`
- Specs → `specs/<module>/<feature>.feature`
- Context → `specs/<module>/<feature>.context.md`
- Data → `specs/<module>/<feature>.data.json`

## Architecture Summary
- `BasePage` — base class for all Page Objects with explicit wait helpers
- `BaseTest` — base class for xUnit fixtures (manages driver lifecycle)
- `WebDriverContext` — SpecFlow DI context (one driver per scenario)
- `BrowserFactory` — creates `IWebDriver` instances (Chrome/Firefox, headless)
- `ConfigManager` — reads `appsettings.json` (overrideable via env vars)
- `DriverHooks` — SpecFlow `[BeforeScenario]`/`[AfterScenario]` hooks

## Agent Modes
- **GENERATE**: spec → C# code → use `agent/prompts/generate.md`
- **HEAL**: test failure → root cause + fix proposal → use `agent/prompts/heal.md`
- **REVIEW**: code → violations + GUIDO Scale score → use `agent/prompts/review.md`

## Build & Test
```bash
# Restore
dotnet restore src/GUIDO.Agentic.Tests.slnx

# Build
dotnet build src/GUIDO.Agentic.Tests.slnx

# Run all tests (headless)
dotnet test src/GUIDO.Agentic.Tests.slnx \
  --configuration Release \
  -e AppSettings__Headless=true
```

## Global Context
See `agent/context/_global.context.md` for the full architectural context.
