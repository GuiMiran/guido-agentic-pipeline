# GUIDO Agentic Pipeline — Global Context

## Project Identity
- **Name**: GUIDO Agentic Pipeline
- **Purpose**: Self-maintaining agentic E2E test automation framework
- **Target Application**: SauceDemo — https://www.saucedemo.com
- **Repository**: https://github.com/GuiMiran/guido-agentic-pipeline
- **Author**: Guido Miranda Mercado — Senior QA Engineering Leader

## Technology Stack

| Layer        | Technology                                 |
|--------------|--------------------------------------------|
| Language     | C# (.NET 8)                                |
| BDD          | SpecFlow 3.9                               |
| Test Runner  | xUnit                                      |
| UI Driver    | Selenium WebDriver 4.x                     |
| Driver Mgmt  | WebDriverManager (no manual chromedriver)  |
| Assertions   | FluentAssertions                           |
| Reporting    | Allure                                     |
| CI/CD        | GitHub Actions                             |
| Pattern      | Page Component Model                       |

## Directory Conventions

| Path                                               | Purpose                              |
|----------------------------------------------------|--------------------------------------|
| `src/GUIDO.Agentic.Tests/Core/`                    | Infrastructure (driver, config, base)|
| `src/GUIDO.Agentic.Tests/Pages/`                   | Page Objects                         |
| `src/GUIDO.Agentic.Tests/Pages/Components/`        | Reusable page components             |
| `src/GUIDO.Agentic.Tests/StepDefinitions/`         | SpecFlow step bindings               |
| `src/GUIDO.Agentic.Tests/Features/`                | SpecFlow `.feature` files            |
| `src/GUIDO.Agentic.Tests/Hooks/`                   | SpecFlow lifecycle hooks             |
| `src/GUIDO.Agentic.Tests/Models/`                  | Data models / DTOs                   |
| `src/GUIDO.Agentic.Tests/Helpers/`                 | Utility helpers (data loaders, etc.) |
| `src/GUIDO.Agentic.Tests/TestData/`                | JSON test data files                 |
| `specs/<module>/`                                  | Human-readable specs (input)         |
| `agent/prompts/`                                   | Agent operational prompts            |
| `agent/tasks/pending/`                             | Queued atomic tasks for the agent    |
| `agent/context/`                                   | Shared context documents             |

## Namespace Convention
`GUIDO.Agentic.Tests.<Layer>` e.g.:
- `GUIDO.Agentic.Tests.Core`
- `GUIDO.Agentic.Tests.Pages`
- `GUIDO.Agentic.Tests.StepDefinitions`

## Hard Rules (Non-negotiable)
1. **NEVER** use `Thread.Sleep` — only `WebDriverWait` / explicit waits.
2. **NEVER** hardcode URLs or credentials — always use `ConfigManager`.
3. **NEVER** generate code without a `.feature` file backing it.
4. **ALL** locators must be declared in the spec's `.context.md`.
5. If a locator is not in context → **ASK**, do not invent.
6. All Page Objects **MUST** inherit from `BasePage`.
7. All step definitions use `IWebDriverContext` via constructor injection.

## Agent Modes
| Mode       | Trigger                         | Prompt File             |
|------------|---------------------------------|-------------------------|
| GENERATE   | New `.feature` + `.context.md`  | `agent/prompts/generate.md` |
| HEAL       | CI test failure                 | `agent/prompts/heal.md`     |
| REVIEW     | PR opened with test code        | `agent/prompts/review.md`   |

## SauceDemo Application Map
| Module    | URL path             | Feature file                          |
|-----------|----------------------|---------------------------------------|
| Auth      | `/`                  | `specs/auth/login.feature`            |
| Inventory | `/inventory.html`    | `specs/inventory/inventory.feature`   |
| Cart      | `/cart.html`         | `specs/cart/cart.feature`             |
| Checkout  | `/checkout-step-one.html` | `specs/checkout/checkout.feature` |
