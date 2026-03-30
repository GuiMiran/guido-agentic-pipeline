Feature: Inventory
  As an authenticated SauceDemo user
  I want to browse the product catalog
  So that I can select items to purchase

  Background:
    Given I am logged in as standard user

  @smoke @inventory
  Scenario: Inventory page displays products
    Then I should see at least 1 product on the page
    And each product should have a name, price and add-to-cart button

  @inventory
  Scenario: Products can be sorted by name ascending
    When I sort products by "Name (A to Z)"
    Then the products should be sorted alphabetically ascending

  @inventory
  Scenario: Products can be sorted by name descending
    When I sort products by "Name (Z to A)"
    Then the products should be sorted alphabetically descending

  @inventory
  Scenario: Products can be sorted by price low to high
    When I sort products by "Price (low to high)"
    Then the products should be sorted by price ascending

  @inventory
  Scenario: Products can be sorted by price high to low
    When I sort products by "Price (high to low)"
    Then the products should be sorted by price descending

  @inventory
  Scenario: Adding a product to cart updates the cart badge
    When I add the first product to the cart
    Then the cart badge should show "1"

  @inventory
  Scenario: Removing a product from cart clears the cart badge
    When I add the first product to the cart
    And I remove the first product from the cart
    Then the cart badge should not be visible
