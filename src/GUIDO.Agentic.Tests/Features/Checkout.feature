Feature: Checkout
  As an authenticated SauceDemo user
  I want to complete the checkout process
  So that I can purchase my selected items

  Background:
    Given I am logged in as standard user
    And I add the first product to the cart from inventory
    When I navigate to the cart page
    And I click checkout

  @smoke @checkout
  Scenario: Complete checkout with valid information
    When I fill in checkout info with first name "John", last name "Doe", and zip "12345"
    And I click continue on checkout
    Then I should be on the checkout overview page
    When I click finish
    Then I should be on the checkout complete page
    And the confirmation header should contain "Thank you for your order!"

  @checkout @negative
  Scenario: Checkout fails with empty form
    When I click continue on checkout
    Then I should see a checkout error containing "First Name is required"

  @checkout @negative
  Scenario: Checkout fails without last name
    When I fill in checkout info with first name "John", last name "", and zip "12345"
    And I click continue on checkout
    Then I should see a checkout error containing "Last Name is required"

  @checkout @negative
  Scenario: Checkout fails without zip code
    When I fill in checkout info with first name "John", last name "Doe", and zip ""
    And I click continue on checkout
    Then I should see a checkout error containing "Postal Code is required"

  @checkout
  Scenario: Checkout overview displays order summary
    When I fill in checkout info with first name "John", last name "Doe", and zip "12345"
    And I click continue on checkout
    Then I should see at least 1 item in the checkout overview
    And the checkout overview should display subtotal, tax, and total

  @checkout
  Scenario: Back Home after order completion returns to inventory
    When I fill in checkout info with first name "John", last name "Doe", and zip "12345"
    And I click continue on checkout
    And I click finish
    And I click back home
    Then I should be on the inventory page
