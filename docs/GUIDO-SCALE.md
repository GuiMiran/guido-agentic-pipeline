# GUIDO Scale — Test Quality Scoring Framework

> **GUIDO Scale** — a governance rubric for evaluating the quality, completeness,
> and maintainability of automated test suites.

## What Is the GUIDO Scale?

The GUIDO Scale scores an automated test suite from **0 to 5** based on five dimensions
aligned with the GUIDO SDD Engineering Stack principles:

| Letter | Dimension      | Question Asked                                          |
|--------|----------------|---------------------------------------------------------|
| **G**  | Governed       | Does the suite follow defined architectural rules?      |
| **U**  | Unified        | Is the suite consistent across modules?                 |
| **I**  | Intent-Driven  | Does each test clearly express its intent in Gherkin?   |
| **D**  | Definition-first | Is every test backed by a spec before the code?       |
| **O**  | Outcomes       | Does the suite validate meaningful business outcomes?   |

## Scoring Rubric

### 5 — Production-Grade
- Zero violations of Hard Rules (no `Thread.Sleep`, no hardcoded values, no invented locators)
- 100% of scenarios have a backing `.feature` + `.context.md`
- All assertions are meaningful (no `true.Should().BeTrue()`)
- Full BDD: Given/When/Then clearly separated
- Negative scenarios present for all critical paths
- GUIDO Scale score reported in CI

### 4 — High Quality
- No violations; minor warnings only
- ≥80% scenario coverage of the module
- At least one negative scenario per feature
- All locators in `.context.md`

### 3 — Acceptable
- 1–2 warnings, no violations
- ≥60% scenario coverage
- Some negative scenarios missing

### 2 — Needs Improvement
- At least one violation present
- Missing `.context.md` for some modules
- Hardcoded values or implicit waits detected

### 1 — Poor
- Multiple violations
- Tests that cannot fail (vacuous assertions)
- No spec files backing generated code

### 0 — Broken / Empty
- Tests do not compile or run
- No meaningful assertions
- No spec files

## How to Calculate Your Score

Run the REVIEW agent prompt (`agent/prompts/review.md`) against your codebase.
The agent will return a structured report and a GUIDO Scale Score.

## Integration with CI

The `run-tests.yml` workflow reports the GUIDO Scale score as a GitHub Check.
A score below **3** blocks merge to `main`.

## Reference
- Original framework: [GUIDO SDD Migration Effort Scale](https://github.com/GuiMiran/guido-sdd-migration-effort-scale)
- Author: **Guido Miranda Mercado** — Senior QA Engineering Leader
