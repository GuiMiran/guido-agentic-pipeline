using FluentAssertions;
using GUIDO.Agentic.Tests.Core;
using GUIDO.Agentic.Tests.Helpers;
using GUIDO.Agentic.Tests.Pages;
using OpenQA.Selenium;
using TechTalk.SpecFlow;

namespace GUIDO.Agentic.Tests.StepDefinitions;

[Binding]
public class InventorySteps
{
    private readonly IWebDriver _driver;
    private readonly InventoryPage _inventoryPage;

    public InventorySteps(DriverContext ctx)
    {
        _driver = ctx.Driver;
        _inventoryPage = new InventoryPage(_driver);
    }

    [Given(@"I am logged in as standard user")]
    public void GivenIAmLoggedInAsStandardUser() =>
        LoginHelper.LoginAsStandardUser(_driver);

    [Then(@"I should see at least 1 product on the page")]
    public void ThenIShouldSeeAtLeast1Product() =>
        _inventoryPage.GetProducts().Count.Should().BeGreaterThan(0);

    [Then(@"each product should have a name, price and add-to-cart button")]
    public void ThenEachProductShouldHaveNamePriceAndButton() =>
        _inventoryPage.EachProductHasNamePriceAndButton().Should().BeTrue();

    [When(@"I sort products by ""(.*)""")]
    public void WhenISortProductsBy(string sortLabel) =>
        _inventoryPage.SortBy(sortLabel);

    [Then(@"the products should be sorted alphabetically ascending")]
    public void ThenProductsSortedAlphaAscending() =>
        _inventoryPage.GetProductNames().Should().BeInAscendingOrder();

    [Then(@"the products should be sorted alphabetically descending")]
    public void ThenProductsSortedAlphaDescending() =>
        _inventoryPage.GetProductNames().Should().BeInDescendingOrder();

    [Then(@"the products should be sorted by price ascending")]
    public void ThenProductsSortedPriceAscending() =>
        _inventoryPage.GetProductPrices().Should().BeInAscendingOrder();

    [Then(@"the products should be sorted by price descending")]
    public void ThenProductsSortedPriceDescending() =>
        _inventoryPage.GetProductPrices().Should().BeInDescendingOrder();

    [When(@"I add the first product to the cart")]
    public void WhenIAddFirstProductToCart() =>
        _inventoryPage.AddFirstProductToCart();

    [Then(@"the cart badge should show ""(.*)""")]
    public void ThenCartBadgeShouldShow(string expected) =>
        _inventoryPage.GetCartBadgeText().Should().Be(expected);

    [When(@"I remove the first product from the cart")]
    public void WhenIRemoveFirstProductFromCart() =>
        _inventoryPage.RemoveFirstProductFromCart();

    [Then(@"the cart badge should not be visible")]
    public void ThenCartBadgeShouldNotBeVisible() =>
        _inventoryPage.IsCartBadgeVisible().Should().BeFalse();
}
