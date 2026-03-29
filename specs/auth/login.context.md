# Login Page — Context Document
# Target: https://www.saucedemo.com
# Module: Authentication (auth)

## URL
- Login page: `https://www.saucedemo.com/`
- Post-login redirect: `https://www.saucedemo.com/inventory.html`

## Locators

| Element          | Selector Type | Selector Value                    | Notes                        |
|------------------|---------------|-----------------------------------|------------------------------|
| Username field   | id            | `user-name`                       | Required, text input         |
| Password field   | id            | `password`                        | Required, password input     |
| Login button     | id            | `login-button`                    | Submit button                |
| Error message    | css           | `[data-test="error"]`             | Visible on auth failure      |
| Error close btn  | css           | `.error-button`                   | X icon to dismiss error      |

## Valid Test Users

| Username                    | Expected Behaviour                         |
|-----------------------------|---------------------------------------------|
| `standard_user`             | Successful login → inventory page          |
| `locked_out_user`           | Error: "Sorry, this user has been locked out." |
| `problem_user`              | Login succeeds but product images broken   |
| `performance_glitch_user`   | Login succeeds with ~5 s delay             |
| `error_user`                | Login succeeds but some actions fail       |
| `visual_user`               | Login succeeds but UI has visual bugs      |

## Common Password
All test users share the password: `secret_sauce`

## Error Messages

| Trigger                        | Expected Error Text                                              |
|--------------------------------|------------------------------------------------------------------|
| Empty username                 | "Epic sadface: Username is required"                            |
| Empty password                 | "Epic sadface: Password is required"                            |
| Invalid credentials            | "Epic sadface: Username and password do not match any user in this service" |
| Locked-out user                | "Epic sadface: Sorry, this user has been locked out."           |

## Agent Instructions
- NEVER invent locators — all selectors must appear in this file.
- If the selector is not listed, ask before proceeding.
- Use `[data-test]` attributes over CSS classes where available.
