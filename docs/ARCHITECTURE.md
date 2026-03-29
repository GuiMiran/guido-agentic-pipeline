# GUIDO Agentic Pipeline — Architecture

## Overview

The GUIDO Agentic Pipeline is a **self-maintaining E2E test automation system** where
AI agents generate, execute, review, and heal test suites from human-authored specs.

```
┌─────────────────────────────────────────────────────────────────────┐
│                        HUMAN INPUT LAYER                            │
│  specs/<module>/                                                    │
│  ├── <feature>.feature     ← Gherkin scenarios                      │
│  ├── <feature>.context.md  ← Locators, URLs, business rules         │
│  └── <feature>.data.json   ← Test data                             │
└──────────────────────────────┬──────────────────────────────────────┘
                               │
                               ▼
┌─────────────────────────────────────────────────────────────────────┐
│                         AGENT LAYER                                 │
│  agent/                                                             │
│  ├── prompts/generate.md   ← Spec → C# code                         │
│  ├── prompts/heal.md       ← Failure → fix proposal                 │
│  ├── prompts/review.md     ← Code → GUIDO Scale score               │
│  ├── tasks/pending/        ← Queued atomic tasks                    │
│  └── context/_global.context.md ← Shared architecture context       │
└──────────────────────────────┬──────────────────────────────────────┘
                               │
                               ▼
┌─────────────────────────────────────────────────────────────────────┐
│                      EXECUTION LAYER (C#)                           │
│  src/GUIDO.Agentic.Tests/                                           │
│  ├── Core/                                                          │
│  │   ├── ConfigManager.cs    ← Centralised config (no hardcoding)   │
│  │   ├── BrowserFactory.cs   ← WebDriver factory (Chrome/Firefox)   │
│  │   ├── BasePage.cs         ← Base Page Object with explicit waits  │
│  │   ├── BaseTest.cs         ← Base test with driver lifecycle       │
│  │   └── WebDriverContext.cs ← SpecFlow DI context                  │
│  ├── Pages/                  ← Page Objects (one per page/module)   │
│  │   ├── Components/         ← Reusable UI components               │
│  │   ├── LoginPage.cs                                               │
│  │   ├── InventoryPage.cs                                           │
│  │   └── CartPage.cs                                                │
│  ├── StepDefinitions/        ← SpecFlow step bindings               │
│  ├── Features/               ← SpecFlow .feature files              │
│  ├── Hooks/                  ← SpecFlow lifecycle hooks             │
│  ├── Models/                 ← Data models / DTOs                   │
│  ├── Helpers/                ← Utility helpers                      │
│  └── TestData/               ← JSON test data                       │
└──────────────────────────────┬──────────────────────────────────────┘
                               │
                               ▼
┌─────────────────────────────────────────────────────────────────────┐
│                        CI/CD LAYER                                  │
│  .github/workflows/                                                 │
│  ├── run-tests.yml       ← Main test pipeline                       │
│  └── agent-heal.yml      ← Auto-healing pipeline (on failure)       │
└──────────────────────────────┬──────────────────────────────────────┘
                               │
                               ▼
┌─────────────────────────────────────────────────────────────────────┐
│                      REPORTING LAYER                                │
│  ├── Allure Report         ← Rich HTML test report                  │
│  ├── GUIDO Scale Score     ← Quality gate (score ≥ 3 to merge)      │
│  └── GitHub Issues         ← Auto-created on failure (heal trigger) │
└─────────────────────────────────────────────────────────────────────┘
```

## Data Flow

```
Spec authored by human
        │
        ▼
GENERATE agent reads .feature + .context.md
        │
        ▼
Agent produces Page Object + Step Definitions
        │
        ▼
GitHub Actions runs tests (headless Chrome)
        │
        ├── PASS → Allure report published
        │
        └── FAIL → agent-heal.yml triggered
                        │
                        ▼
                 HEAL agent analyses failure
                        │
                        ├── HIGH confidence → auto-fix + re-run
                        ├── MEDIUM confidence → PR with fix + review
                        └── LOW confidence → GitHub issue created
```

## Key Design Decisions

### 1. No `Thread.Sleep`
All waits use `WebDriverWait` with explicit conditions. This makes tests resilient
to CI environment variability without arbitrary delays.

### 2. Context-Driven Locators
Locators live in `.context.md` files, not in code. This decouples the app structure
from the test implementation and enables the agent to validate locators before
generating code.

### 3. Dependency Injection for WebDriver
`WebDriverContext` is registered per-scenario via SpecFlow's BoDi container.
This ensures each scenario gets a clean browser and prevents state bleed.

### 4. Spec-First, Code-Second
No test code is generated without a backing `.feature` file. This enforces
the Definition-first principle of the GUIDO Stack.

### 5. Single Source of Truth for Configuration
`ConfigManager` reads from `appsettings.json` (overrideable via environment variables).
CI pipelines override values via `AppSettings__*` env vars.

## Module Coverage Plan

| Module    | Status        | GUIDO Scale |
|-----------|---------------|-------------|
| Auth      | ✅ Implemented | 5           |
| Inventory | 🔲 Planned     | —           |
| Cart      | 🔲 Planned     | —           |
| Checkout  | 🔲 Planned     | —           |
