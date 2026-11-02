Feature: Checkout overview product identity
  As an authenticated SauceDemo user
  I want to review the product I selected
  So that checkout cannot silently replace it with another product

  @checkout
  Scenario: Checkout overview preserves the added product identity
    Given I am logged in as standard user
    And I capture the first inventory product name and price before adding it
    And I add that product to the cart
    And I navigate to the cart and start checkout
    When I fill in checkout info with first name "John", last name "Doe", and zip "12345"
    And I click continue on checkout
    Then the overview should contain exactly one product with the captured name and price
