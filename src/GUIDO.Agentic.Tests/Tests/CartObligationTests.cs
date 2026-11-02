using FluentAssertions;
using GUIDO.Agentic.Tests.Core;
using GUIDO.Agentic.Tests.Helpers;
using GUIDO.Agentic.Tests.Pages;
using Xunit;

namespace GUIDO.Agentic.Tests.Tests;

public class CartObligationTests : BaseTest
{
    [Fact(DisplayName = "Removing the only cart item hides the cart badge"), Trait("Spec", "CART-006")]
    public void RemovingOnlyItemClearsBadge()
    {
        LoginHelper.LoginAsStandardUser(Driver);
        new InventoryPage(Driver).AddFirstProductToCart();
        var cart = new CartPage(Driver).Navigate();
        cart.GetCartItemCount().Should().Be(1);
        cart.GetCartBadgeText().Should().Be("1");

        cart.RemoveFirstItem();
        cart.WaitForCartBadgeToDisappear();

        cart.GetCartItemCount().Should().Be(0);
        cart.IsCartBadgeVisible().Should().BeFalse();
    }
}
