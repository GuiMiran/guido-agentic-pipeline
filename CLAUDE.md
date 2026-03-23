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
