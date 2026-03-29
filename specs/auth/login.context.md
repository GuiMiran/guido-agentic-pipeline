# login.context.md

## Module
Auth — Login

## Target URL
https://www.saucedemo.com

## Business Rules
- Only registered users may access the inventory.
- Locked-out users are rejected with a specific message.
- Empty username or password each produce a distinct required-field error.

## Locators

| Element          | Strategy | Value                          |
|------------------|----------|--------------------------------|
| Username field   | id       | user-name                      |
| Password field   | id       | password                       |
| Login button     | id       | login-button                   |
| Error message    | css      | [data-test="error"]            |
| Page title       | css      | .title                         |

## Acceptance Criteria
- Successful login redirects to a URL containing "inventory".
- Page title element text equals "Products" after successful login.
- Locked-out user sees error containing "locked out".
- Invalid credentials see error containing "Username and password do not match".
- Empty form submission sees error containing "Username is required".
- Username only (no password) sees error containing "Password is required".

## GUIDO Scale Tags
- @guido:L3 — defined standards, structured spec, BDD coverage
