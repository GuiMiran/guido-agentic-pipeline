# Feature: Login — SauceDemo Authentication
# Context: specs/auth/login.context.md
# Data: specs/auth/login.data.json

Feature: Login
  As a registered user
  I want to log into SauceDemo
  So that I can access the product catalogue

  Background:
    Given I navigate to "https://www.saucedemo.com"

  Scenario: Successful login — standard user
    When I log in with username "standard_user" and password "secret_sauce"
    Then I should land on the inventory page

  Scenario: Rejected login — locked-out user
    When I log in with username "locked_out_user" and password "secret_sauce"
    Then I should see a login error containing "locked out"

  Scenario: Rejected login — invalid credentials
    When I log in with username "bad_user" and password "bad_pass"
    Then I should see a login error containing "Username and password do not match"

  Scenario: Required field — empty username
    When I click Login without entering credentials
    Then I should see a login error containing "Username is required"
