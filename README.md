# GUIDO Agentic Pipeline

> Self-maintaining E2E test automation framework powered by the
> GUIDO SDD Engineering Stack and agentic AI.

## What is this?

A framework where AI agents **generate**, **execute**, and **self-heal**
test suites from human-defined specs — without manual intervention.

Built on the **GUIDO Stack**:
**G**overned · **U**nified · **I**ntent-Driven · **D**efinition-first · **O**utcomes

## How it works

```
Spec (Gherkin + .context.md + .data.json)
        ↓
   GUIDO Agent (Claude / Copilot)
        ↓
   C# SpecFlow + Selenium WebDriver
        ↓
   GitHub Actions Pipeline
        ↓
   Allure Report + GUIDO Scale Score
        ↓
   Auto-heal on failure
```

## Stack

| Layer      | Technology                             |
|------------|----------------------------------------|
| Intent     | Gherkin · context.md · data.json       |
| Execution  | Selenium WebDriver · SpecFlow · xUnit · C# (.NET 8) |
| Governance | GUIDO Scale · Coverage Gates           |
| Platform   | GitHub Actions                         |
| Knowledge  | Allure · Living Docs                   |

## Target Application: SauceDemo

Full E2E coverage: **Auth** · **Inventory** · **Cart** · **Checkout**

URL: https://www.saucedemo.com

## Project Structure

```
guido-agentic-pipeline/
├── src/GUIDO.Agentic.Tests/       ← C# test project
│   ├── Core/                      ← BrowserFactory, BasePage, ConfigManager
│   ├── Pages/                     ← Page Objects (LoginPage, InventoryPage, CartPage)
│   ├── StepDefinitions/           ← SpecFlow step bindings
│   ├── Features/                  ← SpecFlow .feature files
│   ├── Hooks/                     ← SpecFlow lifecycle hooks
│   ├── Models/                    ← Data models
│   ├── Helpers/                   ← Utility helpers
│   └── TestData/                  ← JSON test data
│
├── specs/                         ← Human-authored specs (agent input)
│   ├── auth/                      ← login.feature, login.context.md, login.data.json
│   ├── inventory/
│   ├── cart/
│   └── checkout/
│
├── agent/                         ← Agentic system brain
│   ├── prompts/                   ← generate.md, heal.md, review.md
│   ├── tasks/pending/             ← Queued agent tasks
│   └── context/_global.context.md ← Shared architectural context
│
├── .github/workflows/
│   ├── run-tests.yml              ← Main CI pipeline
│   └── agent-heal.yml             ← Auto-healing pipeline
│
├── docs/
│   ├── ARCHITECTURE.md
│   └── GUIDO-SCALE.md
│
└── CLAUDE.md                      ← Claude Code context
```

## Quick Start

```bash
# 1. Restore dependencies
dotnet restore src/GUIDO.Agentic.Tests.slnx

# 2. Build
dotnet build src/GUIDO.Agentic.Tests.slnx

# 3. Run tests (headless Chrome)
dotnet test src/GUIDO.Agentic.Tests.slnx \
  --configuration Release \
  -e AppSettings__Headless=true
```

## Agent Modes

| Mode     | Trigger                        | Prompt File                  |
|----------|--------------------------------|------------------------------|
| GENERATE | New spec files authored        | `agent/prompts/generate.md`  |
| HEAL     | CI test failure                | `agent/prompts/heal.md`      |
| REVIEW   | PR opened with test code       | `agent/prompts/review.md`    |

## GUIDO Scale

Test quality is measured by the **GUIDO Scale** (0–5).
A score below **3** blocks merge to `main`.

See [`docs/GUIDO-SCALE.md`](docs/GUIDO-SCALE.md) for the full rubric.

## Architecture

See [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) for the full system diagram and design decisions.

## Author

**Guido Miranda Mercado** — Senior QA Engineering Leader
Creator of the [GUIDO Scale](https://github.com/GuiMiran/guido-sdd-migration-effort-scale)
[Blog](https://guidomiranda.wordpress.com)