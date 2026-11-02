using System.Text.Json;
using FluentAssertions;
using GUIDO.Agentic.Tests.Core;
using GUIDO.Agentic.Tests.Helpers;
using GUIDO.Agentic.Tests.Pages;
using Xunit;

namespace GUIDO.Agentic.Tests.Tests;

public class CheckoutObligationTests : BaseTest
{
    [Fact(DisplayName = "Checkout overview preserves the added product identity"), Trait("Spec", "CHECKOUT-007")]
    public void OverviewPreservesProductIdentity()
    {
        LoginHelper.LoginAsStandardUser(Driver);
        var inventory = new InventoryPage(Driver).WaitForLoad();
        var expectedName = inventory.GetProductNames()[0];
        var expectedPrice = inventory.GetProductPrices()[0];
        expectedName.Should().NotBeNullOrWhiteSpace();
        expectedPrice.Should().BeGreaterThan(0);
        inventory.AddFirstProductToCart();
        var cart = new CartPage(Driver).Navigate();
        cart.GetCartItemCount().Should().Be(1);
        cart.ClickCheckout();

        using var data = JsonDocument.Parse(File.ReadAllText(
            Path.Combine(AppContext.BaseDirectory, "TestData", "checkout.json")));
        var input = data.RootElement;
        var checkout = new CheckoutPage(Driver).WaitForStepOne();
        checkout.EnterFirstName(input.GetProperty("firstName").GetString()!)
            .EnterLastName(input.GetProperty("lastName").GetString()!)
            .EnterZip(input.GetProperty("zip").GetString()!);
        checkout.ClickContinue();
        checkout.WaitForOverview();

        var items = checkout.GetOverviewProducts();
        items.Should().ContainSingle();
        items[0].Name.Should().Be(expectedName);
        items[0].Price.Should().Be(expectedPrice);
    }
}
