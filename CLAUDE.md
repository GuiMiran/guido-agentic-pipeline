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
- NEVER use Thread.Sleep — only WebDriverWait / explicit waits
- NEVER hardcode URLs or credentials — use ConfigManager
- NEVER generate code without a .feature backing it
- ALL locators must exist in the feature's .context.md
- If a locator is not in context → ASK, do not invent

## File Conventions
- Pages → /src/GUIDO.Agentic.Tests/Pages/[Module]Page.cs
- Page Components → /src/GUIDO.Agentic.Tests/Pages/Components/[Component].cs
- Steps → /src/GUIDO.Agentic.Tests/StepDefinitions/[Module]Steps.cs
- Specs → /specs/[module]/[feature].feature
- Context → /specs/[module]/[feature].context.md

## Agent Modes
- GENERATE: spec → C# code
- HEAL: test failure → root cause + fix proposal
- REVIEW: code → violations + recommendations
- COVERAGE: specs + code → GUIDO Scale score
