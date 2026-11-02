using System.Text.Json;
using FluentAssertions;
using GUIDO.Agentic.Tests.Core;
using GUIDO.Agentic.Tests.Helpers;
using GUIDO.Agentic.Tests.Pages;
using Xunit;

namespace GUIDO.Agentic.Tests.Tests;

// One fresh BaseTest/WebDriver per xUnit fact. Specs remain documentation inputs.
public class AuthTests : BaseTest
{
    private readonly LoginPage _login;
    public AuthTests() { _login = new LoginPage(Driver); _login.Open(); }
    private void ExpectError(string message) { _login.HasErrorMessage().Should().BeTrue(); _login.GetErrorMessage().Should().ContainEquivalentOf(message); }
    [Fact(DisplayName = "Successful login with standard user"), Trait("Spec", "AUTH-001")]
    public void StandardLogin() { _login.Login(ConfigManager.StandardUser, ConfigManager.Password); var page = new InventoryPage(Driver).WaitForLoad(); Driver.Url.Should().Contain("inventory.html"); page.GetTitle().Should().Be("Products"); }
    [Fact(DisplayName = "Login with locked-out user is rejected"), Trait("Spec", "AUTH-002")]
    public void LockedOut() { _login.Login(ConfigManager.LockedOutUser, ConfigManager.Password); ExpectError("locked out"); }
    [Fact(DisplayName = "Login with invalid credentials shows error"), Trait("Spec", "AUTH-003")]
    public void InvalidCredentials() {
        using var data = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "TestData", "auth-cases.json")));
        var invalid = data.RootElement.GetProperty("invalidUsers")[1];
        _login.Login(invalid.GetProperty("username").GetString()!, invalid.GetProperty("password").GetString()!);
        ExpectError("Username and password do not match");
    }
    [Fact(DisplayName = "Login without credentials shows required field error"), Trait("Spec", "AUTH-004")]
    public void EmptyCredentials() { _login.ClickLogin(); ExpectError("Username is required"); }
    [Fact(DisplayName = "Login with username only shows password required error"), Trait("Spec", "AUTH-005")]
    public void MissingPassword() { _login.EnterUsername(ConfigManager.StandardUser); _login.ClickLogin(); ExpectError("Password is required"); }
}

public class InventoryTests : BaseTest
{
    private readonly InventoryPage _page;
    public InventoryTests() { LoginHelper.LoginAsStandardUser(Driver); _page = new InventoryPage(Driver).WaitForLoad(); }
    [Fact(DisplayName = "Inventory page displays products"), Trait("Spec", "INVENTORY-001")]
    public void Products() { _page.GetProducts().Count.Should().BeGreaterThan(0); _page.EachProductHasNamePriceAndButton().Should().BeTrue(); }
    [Fact(DisplayName = "Products can be sorted by name ascending"), Trait("Spec", "INVENTORY-002")]
    public void NameAscending() { _page.SortBy("Name (A to Z)"); _page.GetProductNames().Should().BeInAscendingOrder(); }
    [Fact(DisplayName = "Products can be sorted by name descending"), Trait("Spec", "INVENTORY-003")]
    public void NameDescending() { _page.SortBy("Name (Z to A)"); _page.GetProductNames().Should().BeInDescendingOrder(); }
    [Fact(DisplayName = "Products can be sorted by price low to high"), Trait("Spec", "INVENTORY-004")]
    public void PriceAscending() { _page.SortBy("Price (low to high)"); _page.GetProductPrices().Should().BeInAscendingOrder(); }
    [Fact(DisplayName = "Products can be sorted by price high to low"), Trait("Spec", "INVENTORY-005")]
    public void PriceDescending() { _page.SortBy("Price (high to low)"); _page.GetProductPrices().Should().BeInDescendingOrder(); }
    [Fact(DisplayName = "Adding a product to cart updates the cart badge"), Trait("Spec", "INVENTORY-006")]
    public void Add() { _page.AddFirstProductToCart(); _page.GetCartBadgeText().Should().Be("1"); }
    [Fact(DisplayName = "Removing a product from cart clears the cart badge"), Trait("Spec", "INVENTORY-007")]
    public void Remove() { _page.AddFirstProductToCart(); _page.RemoveFirstProductFromCart(); _page.IsCartBadgeVisible().Should().BeFalse(); }
}

public class CartTests : BaseTest
{
    private readonly InventoryPage _inventory;
    private readonly CartPage _cart;
    public CartTests() { LoginHelper.LoginAsStandardUser(Driver); _inventory = new InventoryPage(Driver); _cart = new CartPage(Driver); }
    private void OpenWithOne() { _inventory.AddFirstProductToCart(); _cart.Navigate(); }
    [Fact(DisplayName = "Cart page shows added item"), Trait("Spec", "CART-001")]
    public void AddedItem() { OpenWithOne(); _cart.GetCartItemCount().Should().Be(1); _cart.EachItemHasNameQuantityAndPrice().Should().BeTrue(); }
    [Fact(DisplayName = "Cart badge matches item count"), Trait("Spec", "CART-002")]
    public void TwoItems() { _inventory.AddProductsToCart(2); _cart.Navigate(); _cart.GetCartItemCount().Should().Be(2); }
    [Fact(DisplayName = "Removing an item from cart updates the list"), Trait("Spec", "CART-003")]
    public void RemoveItem() { OpenWithOne(); _cart.RemoveFirstItem(); _cart.IsCartEmpty().Should().BeTrue(); }
    [Fact(DisplayName = "Continue shopping returns to inventory"), Trait("Spec", "CART-004")]
    public void ContinueShopping() { OpenWithOne(); _cart.ClickContinueShopping(); _cart.GetCurrentUrl().Should().Contain("inventory"); }
    [Fact(DisplayName = "Checkout button navigates to checkout"), Trait("Spec", "CART-005")]
    public void Checkout() { OpenWithOne(); _cart.ClickCheckout(); _cart.GetCurrentUrl().Should().Contain("checkout-step-one"); }
}

public class CheckoutTests : BaseTest
{
    private readonly CheckoutPage _checkout;
    public CheckoutTests() { LoginHelper.LoginAsStandardUser(Driver); new InventoryPage(Driver).AddFirstProductToCart(); var cart = new CartPage(Driver).Navigate(); cart.ClickCheckout(); _checkout = new CheckoutPage(Driver).WaitForStepOne(); }
    private void Fill(bool last = true, bool zip = true) {
        using var data = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "TestData", "checkout.json")));
        var input = data.RootElement;
        _checkout.EnterFirstName(input.GetProperty("firstName").GetString()!).EnterLastName(last ? input.GetProperty("lastName").GetString()! : "").EnterZip(zip ? input.GetProperty("zip").GetString()! : "");
    }
    [Fact(DisplayName = "Complete checkout with valid information"), Trait("Spec", "CHECKOUT-001")]
    public void Complete() { Fill(); _checkout.ClickContinue(); _checkout.GetCurrentUrl().Should().Contain("checkout-step-two"); _checkout.ClickFinish(); _checkout.GetCurrentUrl().Should().Contain("checkout-complete"); _checkout.GetCompleteHeader().Should().Contain("Thank you for your order!"); }
    [Fact(DisplayName = "Checkout fails with empty form"), Trait("Spec", "CHECKOUT-002")]
    public void EmptyForm() { _checkout.ClickContinue(); _checkout.GetErrorMessage().Should().Contain("First Name is required"); }
    [Fact(DisplayName = "Checkout fails without last name"), Trait("Spec", "CHECKOUT-003")]
    public void MissingLastName() { Fill(last: false); _checkout.ClickContinue(); _checkout.GetErrorMessage().Should().Contain("Last Name is required"); }
    [Fact(DisplayName = "Checkout fails without zip code"), Trait("Spec", "CHECKOUT-004")]
    public void MissingZip() { Fill(zip: false); _checkout.ClickContinue(); _checkout.GetErrorMessage().Should().Contain("Postal Code is required"); }
    [Fact(DisplayName = "Checkout overview displays order summary"), Trait("Spec", "CHECKOUT-005")]
    public void Summary() { Fill(); _checkout.ClickContinue(); _checkout.HasOverviewItems().Should().BeTrue(); _checkout.HasOrderSummary().Should().BeTrue(); }
    [Fact(DisplayName = "Back Home after order completion returns to inventory"), Trait("Spec", "CHECKOUT-006")]
    public void BackHome() { Fill(); _checkout.ClickContinue(); _checkout.ClickFinish(); _checkout.ClickBackHome(); _checkout.GetCurrentUrl().Should().Contain("inventory"); }
}
