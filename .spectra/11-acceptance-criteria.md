# Pipeline acceptance scope — SPECTRA layer 11

These stable IDs normalize existing documented scenarios and context obligations.
They are proposed links for owner review, not new business rules or proof of
complete application coverage. The original specs remain unchanged.

The original 23 criteria retain their assertion scope. Four further criteria
have dedicated test declarations; AC-025 now also has a dedicated description Fact.
Test declarations require fresh execution evidence. More untested rules may exist;
28 is a reviewed local inventory, not an exhaustive product denominator.
No static mapping, trait or generated inventory counts as execution evidence.

## AC-001 — Successful standard login

With the configured standard user and password, login reaches a URL containing inventory.html and the inventory title is Products.

Source: `specs/auth/login.feature` — scenario `Successful login with standard user`.
Declared test: `AUTH-001` / `AuthTests.StandardLogin`.

## AC-002 — Locked-out login error

With the configured locked-out user, login presents an error containing locked out.

Source: `specs/auth/login.feature` — scenario `Login with locked-out user is rejected`.
Declared test: `AUTH-002` / `AuthTests.LockedOut`.

## AC-003 — Invalid-credentials error

The documented invalid-user case presents an error containing Username and password do not match.

Source: `specs/auth/login.feature` — scenario `Login with invalid credentials shows error`.
Declared test: `AUTH-003` / `AuthTests.InvalidCredentials`.

## AC-004 — Missing-username error

Submitting login with both credentials empty presents Username is required.

Source: `specs/auth/login.feature` — scenario `Login without credentials shows required field error`.
Declared test: `AUTH-004` / `AuthTests.EmptyCredentials`.

## AC-005 — Missing-password error

Submitting only the standard username presents Password is required.

Source: `specs/auth/login.feature` — scenario `Login with username only shows password required error`.
Declared test: `AUTH-005` / `AuthTests.MissingPassword`.

## AC-006 — Inventory product fields

After standard-user login, at least one product is present and each has a non-empty name, a price starting with $, and an add-to-cart button. This does not establish the six-product obligation AC-024.

Source: `specs/inventory/inventory.feature` — scenario `Inventory page displays products`.
Declared test: `INVENTORY-001` / `InventoryTests.Products`.

## AC-007 — Ascending product names

Selecting Name (A to Z) yields product names in ascending order.

Source: `specs/inventory/inventory.feature` — scenario `Products can be sorted by name ascending`.
Declared test: `INVENTORY-002` / `InventoryTests.NameAscending`.

## AC-008 — Descending product names

Selecting Name (Z to A) yields product names in descending order.

Source: `specs/inventory/inventory.feature` — scenario `Products can be sorted by name descending`.
Declared test: `INVENTORY-003` / `InventoryTests.NameDescending`.

## AC-009 — Ascending product prices

Selecting Price (low to high) yields parsed product prices in ascending order.

Source: `specs/inventory/inventory.feature` — scenario `Products can be sorted by price low to high`.
Declared test: `INVENTORY-004` / `InventoryTests.PriceAscending`.

## AC-010 — Descending product prices

Selecting Price (high to low) yields parsed product prices in descending order.

Source: `specs/inventory/inventory.feature` — scenario `Products can be sorted by price high to low`.
Declared test: `INVENTORY-005` / `InventoryTests.PriceDescending`.

## AC-011 — First add badge

Adding the first product makes the cart badge read 1.

Source: `specs/inventory/inventory.feature` — scenario `Adding a product to cart updates the cart badge`.
Declared test: `INVENTORY-006` / `InventoryTests.Add`.

## AC-012 — Inventory removal badge

Adding then removing the first product from inventory leaves no visible cart badge.

Source: `specs/inventory/inventory.feature` — scenario `Removing a product from cart clears the cart badge`.
Declared test: `INVENTORY-007` / `InventoryTests.Remove`.

## AC-013 — One-item cart fields

After adding one product, the cart has exactly one item with a non-empty name, quantity 1, and a price starting with $.

Source: `specs/cart/cart.feature` — scenario `Cart page shows added item`.
Declared test: `CART-001` / `CartTests.AddedItem`.

## AC-014 — Two-item cart count

Starting with an empty cart and adding two products yields a badge count of 1 then 2 during the multi-add helper and exactly two cart rows after navigation. This is the two-product example, not arbitrary-count equality.

Source: `specs/cart/cart.feature` — scenario `Cart badge matches item count`.
Declared test: `CART-002` / `CartTests.TwoItems`.

## AC-015 — Cart row removal

Removing the only cart item leaves the cart list empty. Badge disappearance in this cart-page flow is a separate unverified obligation AC-026.

Source: `specs/cart/cart.feature` — scenario `Removing an item from cart updates the list`.
Declared test: `CART-003` / `CartTests.RemoveItem`.

## AC-016 — Continue shopping navigation

From a one-item cart, Continue Shopping reaches a URL containing inventory.

Source: `specs/cart/cart.feature` — scenario `Continue shopping returns to inventory`.
Declared test: `CART-004` / `CartTests.ContinueShopping`.

## AC-017 — Checkout navigation

From a one-item cart, Checkout reaches a URL containing checkout-step-one.

Source: `specs/cart/cart.feature` — scenario `Checkout button navigates to checkout`.
Declared test: `CART-005` / `CartTests.Checkout`.

## AC-018 — Successful checkout

With the configured valid checkout data, Continue reaches checkout-step-two and Finish reaches checkout-complete with a header containing Thank you for your order!.

Source: `specs/checkout/checkout.feature` — scenario `Complete checkout with valid information`.
Declared test: `CHECKOUT-001` / `CheckoutTests.Complete`.

## AC-019 — Empty checkout form

Submitting an empty checkout form presents First Name is required.

Source: `specs/checkout/checkout.feature` — scenario `Checkout fails with empty form`.
Declared test: `CHECKOUT-002` / `CheckoutTests.EmptyForm`.

## AC-020 — Missing last name

Submitting valid first name and postal code with no last name presents Last Name is required.

Source: `specs/checkout/checkout.feature` — scenario `Checkout fails without last name`.
Declared test: `CHECKOUT-003` / `CheckoutTests.MissingLastName`.

## AC-021 — Missing postal code

Submitting valid first and last names with no postal code presents Postal Code is required.

Source: `specs/checkout/checkout.feature` — scenario `Checkout fails without zip code`.
Declared test: `CHECKOUT-004` / `CheckoutTests.MissingZip`.

## AC-022 — Overview presence

With valid checkout information, overview has at least one item and subtotal, tax and total elements exist. This does not establish item identity (AC-028) or numeric correctness.

Source: `specs/checkout/checkout.feature` — scenario `Checkout overview displays order summary`.
Declared test: `CHECKOUT-005` / `CheckoutTests.Summary`.

## AC-023 — Back Home navigation

After completing an order, Back Home reaches a URL containing inventory.

Source: `specs/checkout/checkout.feature` — scenario `Back Home after order completion returns to inventory`.
Declared test: `CHECKOUT-006` / `CheckoutTests.BackHome`.

## AC-024 — Exact standard-user inventory count

The standard-user inventory shows exactly 6 products.

Source: `specs/inventory/inventory.context.md`.
Declared test: `INVENTORY-008` / `InventoryObligationTests.ExactProductCount`.
Backing scenario: `specs/inventory/inventory.feature` — `Standard user inventory contains exactly six products`.

## AC-025 — Product descriptions

Every inventory product displays its description as required by the inventory business rules.

Source: `specs/inventory/inventory.context.md`.
Declared test: `INVENTORY-009` / `InventoryObligationTests.ProductDescriptions`.
Backing scenario: `specs/inventory/inventory.feature` — `Every inventory product displays a non-empty description`.
Scope: exactly one visible non-empty description per product; not editorial correctness or completeness of its prose. Selector verification requires fresh CI after owner authorization on 2026-11-03.

## AC-026 — Cart-page removal badge

Removing the only item from the cart page leaves an empty list and the cart badge disappears.

Source: `specs/cart/cart.context.md`.
Declared test: `CART-006` / `CartObligationTests.RemovingOnlyItemClearsBadge`.
Backing scenario: `specs/cart/cart.feature` — `Removing the only cart item hides the cart badge`.

## AC-027 — Unauthenticated inventory access

Inventory access is restricted to authenticated users.

Source: `specs/inventory/inventory.context.md`.
Declared test: `AUTH-006` / `UnauthorizedInventoryTests.DeniesUnauthenticatedInventory`.
Backing scenario: `specs/auth/login.feature` — `Unauthenticated inventory access returns to login`.

## AC-028 — Overview item identity

The checkout overview displays the item added to the cart, not merely any item.

Source: `specs/checkout/checkout.context.md`.
Declared test: `CHECKOUT-007` / `CheckoutObligationTests.OverviewPreservesProductIdentity`.
Backing scenario: `specs/checkout/overview-identity.feature` — `Checkout overview preserves the added product identity`.
