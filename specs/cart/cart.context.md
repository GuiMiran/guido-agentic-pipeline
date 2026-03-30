# cart.context.md

## Module
Cart — Shopping cart management

## Target URL
https://www.saucedemo.com/cart.html

## Business Rules
- Cart is accessible only to authenticated users.
- Each cart item displays name, quantity (always 1 per add), and unit price.
- Removing an item from the cart decrements the badge count.
- An empty cart shows no items and the badge disappears.
- "Continue Shopping" returns to /inventory.html.
- "Checkout" navigates to /checkout-step-one.html.

## Locators

| Element               | Strategy | Value                        |
|-----------------------|----------|------------------------------|
| Cart item list        | css      | .cart_list                   |
| Cart item             | css      | .cart_item                   |
| Item name             | css      | .inventory_item_name         |
| Item quantity         | css      | .cart_quantity               |
| Item price            | css      | .inventory_item_price        |
| Remove button         | css      | .cart_button                 |
| Continue shopping btn | css      | [data-test="continue-shopping"] |
| Checkout button       | css      | [data-test="checkout"]       |
| Cart badge            | css      | .shopping_cart_badge         |

## Acceptance Criteria
- After adding 1 product, cart shows exactly 1 item.
- After adding 2 products, cart shows exactly 2 items.
- Each item has non-empty name, quantity "1", and price starting with "$".
- After removing the only item, cart list is empty and badge disappears.
- "Continue Shopping" redirects to URL containing "inventory".
- "Checkout" redirects to URL containing "checkout-step-one".

## GUIDO Scale Tags
- @guido:L3 — defined standards, structured spec, BDD coverage
