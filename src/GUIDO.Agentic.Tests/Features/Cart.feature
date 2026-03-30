Feature: Cart
  As an authenticated SauceDemo user
  I want to manage items in my shopping cart
  So that I can review and adjust my order before checkout

  Background:
    Given I am logged in as standard user

  @smoke @cart
  Scenario: Cart page shows added item
    Given I add the first product to the cart from inventory
    When I navigate to the cart page
    Then I should see 1 item in the cart
    And the cart item should have a name, quantity and price

  @cart
  Scenario: Cart badge matches item count
    Given I add 2 products to the cart from inventory
    When I navigate to the cart page
    Then I should see 2 items in the cart

  @cart
  Scenario: Removing an item from cart updates the list
    Given I add the first product to the cart from inventory
    When I navigate to the cart page
    And I remove the item from the cart
    Then the cart should be empty

  @cart
  Scenario: Continue shopping returns to inventory
    Given I add the first product to the cart from inventory
    When I navigate to the cart page
    And I click continue shopping
    Then I should be on the inventory page

  @cart
  Scenario: Checkout button navigates to checkout
    Given I add the first product to the cart from inventory
    When I navigate to the cart page
    And I click checkout
    Then I should be on the checkout information page
