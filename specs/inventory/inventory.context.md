# inventory.context.md

## Module
Inventory — Product catalog

## Target URL
https://www.saucedemo.com/inventory.html

## Business Rules
- Only authenticated users can access the inventory page.
- All products must display name, price, description, and add-to-cart button.
- Sorting applies immediately on selection — no submit action required.
- Cart badge reflects the exact count of items added.
- Cart badge disappears when count reaches 0.

## Locators

| Element                  | Strategy | Value                                          |
|--------------------------|----------|------------------------------------------------|
| Product list             | css      | .inventory_list                                |
| Product item             | css      | .inventory_item                                |
| Product name             | css      | .inventory_item_name                           |
| Product price            | css      | .inventory_item_price                          |
| Add to cart button       | css      | .btn_inventory                                 |
| Remove button            | css      | .btn_secondary.btn_inventory                   |
| Sort dropdown            | css      | .product_sort_container                        |
| Cart badge               | css      | .shopping_cart_badge                           |
| Cart link                | css      | .shopping_cart_link                            |
| Page title               | css      | .title                                         |

## Sort Option Values

| Label               | value attribute  |
|---------------------|------------------|
| Name (A to Z)       | az               |
| Name (Z to A)       | za               |
| Price (low to high) | lohi             |
| Price (high to low) | hilo             |

## Acceptance Criteria
- Inventory page shows 6 products for standard_user.
- Each product has non-empty name, price starting with "$", and a button.
- After sorting A→Z, first product name is alphabetically before last.
- After sorting Z→A, first product name is alphabetically after last.
- After sorting price low→high, first price ≤ last price.
- After sorting price high→low, first price ≥ last price.
- Cart badge increments by 1 per product added.
- Cart badge disappears after all items removed.

## GUIDO Scale Tags
- @guido:L3 — defined standards, structured spec, BDD coverage
