using FluentAssertions;
using GUIDO.Agentic.Tests.Core;
using GUIDO.Agentic.Tests.Pages;
using OpenQA.Selenium;
using TechTalk.SpecFlow;

namespace GUIDO.Agentic.Tests.StepDefinitions;

[Binding]
public class CartSteps
{
    private readonly IWebDriver _driver;
    private readonly InventoryPage _inventoryPage;
    private readonly CartPage _cartPage;

    public CartSteps(DriverContext ctx)
    {
        _driver = ctx.Driver;
        _inventoryPage = new InventoryPage(_driver);
        _cartPage = new CartPage(_driver);
    }

    [Given(@"I add the first product to the cart from inventory")]
    public void GivenIAddFirstProductToCartFromInventory() =>
        _inventoryPage.AddFirstProductToCart();

    [Given(@"I add (\d+) products to the cart from inventory")]
    public void GivenIAddNProductsToCartFromInventory(int count)
    {
        var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(_driver,
            TimeSpan.FromSeconds(ConfigManager.TimeoutSeconds));

        for (int i = 0; i < count; i++)
        {
            var button = wait.Until(d =>
                d.FindElements(By.CssSelector(".btn_inventory"))
                 .FirstOrDefault(b => b.Text == "Add to cart"));
            button!.Click();
            // wait for badge to reflect the click before next iteration
            var expected = (i + 1).ToString();
            wait.Until(d => d.FindElement(By.CssSelector(".shopping_cart_badge")).Text == expected);
        }
    }

    [When(@"I navigate to the cart page")]
    public void WhenINavigateToCartPage() => _cartPage.Navigate();

    [Then(@"I should see (\d+) item(?:s)? in the cart")]
    public void ThenIShouldSeeNItemsInCart(int expected) =>
        _cartPage.GetCartItemCount().Should().Be(expected);

    [Then(@"the cart item should have a name, quantity and price")]
    public void ThenCartItemShouldHaveNameQuantityAndPrice() =>
        _cartPage.EachItemHasNameQuantityAndPrice().Should().BeTrue();

    [When(@"I remove the item from the cart")]
    public void WhenIRemoveItemFromCart() => _cartPage.RemoveFirstItem();

    [Then(@"the cart should be empty")]
    public void ThenCartShouldBeEmpty() =>
        _cartPage.IsCartEmpty().Should().BeTrue();

    [When(@"I click continue shopping")]
    public void WhenIClickContinueShopping() => _cartPage.ClickContinueShopping();

    [Then(@"I should be on the inventory page")]
    public void ThenIShouldBeOnInventoryPage() =>
        _cartPage.GetCurrentUrl().Should().Contain("inventory");

    [When(@"I click checkout")]
    public void WhenIClickCheckout() => _cartPage.ClickCheckout();

    [Then(@"I should be on the checkout information page")]
    public void ThenIShouldBeOnCheckoutInformationPage() =>
        _cartPage.GetCurrentUrl().Should().Contain("checkout-step-one");
}
