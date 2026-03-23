Feature: Login
  As a registered SauceDemo user
  I want to authenticate with valid credentials
  So that I can access the product inventory

  Background:
    Given I am on the SauceDemo login page

  @smoke @auth
  Scenario: Successful login with standard user
    When I enter username "standard_user" and password "secret_sauce"
    And I click the login button
    Then I should be redirected to the inventory page
    And the page title should be "Products"

  @auth @negative
  Scenario: Login with locked-out user is rejected
    When I enter username "locked_out_user" and password "secret_sauce"
    And I click the login button
    Then I should see an error message containing "locked out"

  @auth @negative
  Scenario: Login with invalid credentials shows error
    When I enter username "invalid_user" and password "wrong_password"
    And I click the login button
    Then I should see an error message containing "Username and password do not match"

  @auth @negative
  Scenario: Login without credentials shows required field error
    When I click the login button
    Then I should see an error message containing "Username is required"

  @auth @negative
  Scenario: Login with username only shows password required error
    When I enter username "standard_user" and password ""
    And I click the login button
    Then I should see an error message containing "Password is required"
