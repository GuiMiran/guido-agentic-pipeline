# checkout.context.md

## Module
Checkout — Order completion flow

## Target URLs
- Step 1 (Info):     https://www.saucedemo.com/checkout-step-one.html
- Step 2 (Overview): https://www.saucedemo.com/checkout-step-two.html
- Complete:          https://www.saucedemo.com/checkout-complete.html

## Business Rules
- Checkout is only accessible to authenticated users with items in cart.
- Step 1 requires First Name, Last Name, and Zip/Postal Code — all mandatory.
- Empty form submission on Step 1 triggers a "First Name is required" error.
- Missing Last Name triggers "Last Name is required".
- Missing Zip triggers "Postal Code is required".
- Step 2 displays all cart items with subtotal, tax, and total price.
- Finish button on Step 2 completes the order and navigates to checkout-complete.
- Confirmation page displays a success header and thank-you text.
- "Back Home" button on confirmation returns to /inventory.html.

## Locators

### Step 1 — Checkout Info

| Element          | Strategy | Value                      |
|------------------|----------|----------------------------|
| First Name field | css      | [data-test="firstName"]    |
| Last Name field  | css      | [data-test="lastName"]     |
| Zip/Postal field | css      | [data-test="postalCode"]   |
| Continue button  | css      | [data-test="continue"]     |
| Cancel button    | css      | [data-test="cancel"]       |
| Error message    | css      | [data-test="error"]        |

### Step 2 — Checkout Overview

| Element        | Strategy | Value                       |
|----------------|----------|-----------------------------|
| Cart items     | css      | .cart_item                  |
| Item name      | css      | .inventory_item_name        |
| Item price     | css      | .inventory_item_price       |
| Subtotal label | css      | .summary_subtotal_label     |
| Tax label      | css      | .summary_tax_label          |
| Total label    | css      | .summary_total_label        |
| Finish button  | css      | [data-test="finish"]        |
| Cancel button  | css      | [data-test="cancel"]        |

### Step 3 — Checkout Complete

| Element          | Strategy | Value                          |
|------------------|----------|--------------------------------|
| Complete header  | css      | .complete-header               |
| Complete text    | css      | .complete-text                 |
| Back Home button | css      | [data-test="back-to-products"] |

## Acceptance Criteria
- Submitting Step 1 with all fields filled navigates to checkout-step-two.
- Submitting Step 1 with no fields shows error "First Name is required".
- Submitting Step 1 without Last Name shows error "Last Name is required".
- Submitting Step 1 without Zip shows error "Postal Code is required".
- Step 2 overview displays the item added to cart.
- Step 2 shows subtotal, tax, and total labels.
- Clicking Finish navigates to checkout-complete.
- Confirmation page header contains "Thank you for your order!".
- Clicking Back Home returns to URL containing "inventory".

## GUIDO Scale Tags
- @guido:L3 — defined standards, structured spec, BDD coverage
