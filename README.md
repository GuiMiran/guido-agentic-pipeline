# GUIDO Agentic Pipeline

> Self-maintaining E2E test automation framework powered by the
> GUIDO SDD Engineering Stack and agentic AI.

## What is this?

A framework where AI agents generate, execute, and self-heal
test suites from human-defined specs — without manual intervention.

Built on the **GUIDO Stack**:
**G**overned · **U**nified · **I**ntent-Driven · **D**efinition-first · **O**utcomes

## How it works

```
Spec (Gherkin + .context.md)
        ↓
   GUIDO Agent (Claude API)
        ↓
   C# SpecFlow + Selenium
        ↓
   GitHub Actions Pipeline
        ↓
   Allure Report + GUIDO Scale Score
        ↓
   Auto-heal on failure
```

## Stack

| Layer      | Technology                          |
|------------|-------------------------------------|
| Intent     | Gherkin · context.md · data.json    |
| Execution  | Selenium · SpecFlow · xUnit · C#    |
| Governance | GUIDO Scale · Coverage Gates        |
| Platform   | GitHub Actions · Docker             |
| Knowledge  | Allure · Living Docs                |

## Target: SauceDemo (saucedemo.com)

Full E2E coverage: Auth · Inventory · Cart · Checkout

## Author

**Guido Miranda Mercado** — Senior QA Engineering Leader
Creator of the <a href="https://github.com/GuiMiran/guido-sdd-migration-effort-scale">GUIDO Scale</a>
<a href="https://www.linkedin.com/in/guidomirandamercado">LinkedIn</a> · <a href="https://guidomiranda.wordpress.com">Blog</a>
