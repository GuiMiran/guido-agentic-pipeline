using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using GUIDO.Agentic.Tests.Core;

namespace GUIDO.Agentic.Tests.Pages;

/// <summary>
/// Page Object for the SauceDemo cart page (/cart.html).
/// All locators are sourced from specs/cart/cart.context.md.
/// </summary>
public class CartPage : BasePage
{
    // Locators
    private static readonly By PageTitle = By.CssSelector(".title");
    private static readonly By CartItems = By.CssSelector(".cart_item");
    private static readonly By ItemName = By.CssSelector(".inventory_item_name");
    private static readonly By ItemQuantity = By.CssSelector(".cart_quantity");
    private static readonly By ItemPrice = By.CssSelector(".inventory_item_price");
    private static readonly By CheckoutButton = By.CssSelector("[data-test='checkout']");
    private static readonly By ContinueShoppingButton = By.CssSelector("[data-test='continue-shopping']");
    private static readonly By RemoveButtons = By.CssSelector(".cart_button");

    private readonly IJavaScriptExecutor _js;

    public CartPage(IWebDriver driver) : base(driver)
    {
        _js = (IJavaScriptExecutor)driver;
    }

    /// <summary>Navigates to the cart page and waits until it has loaded.</summary>
    public CartPage Navigate()
    {
        NavigateTo("cart.html");
        return WaitForLoad();
    }

    /// <summary>Waits until the cart page has fully loaded.</summary>
    public CartPage WaitForLoad()
    {
        WaitForUrl("cart.html");
        WaitForElement(PageTitle);
        return this;
    }

    /// <summary>Returns the page title text.</summary>
    public string GetTitle() => WaitForElement(PageTitle).Text;

    /// <summary>Returns all cart item elements.</summary>
    public IReadOnlyCollection<IWebElement> GetCartItems() =>
        Driver.FindElements(CartItems);

    /// <summary>Returns the number of items currently in the cart.</summary>
    public int GetItemCount() => GetCartItems().Count;

    /// <summary>Returns the number of items currently in the cart.</summary>
    public int GetCartItemCount() => GetItemCount();

    /// <summary>Returns true when the cart contains at least one item.</summary>
    public bool HasItems() => GetItemCount() > 0;

    /// <summary>Returns true when the cart contains no items.</summary>
    public bool IsCartEmpty() => !HasItems();

    /// <summary>Returns true when each item has name, quantity, and price populated.</summary>
    public bool EachItemHasNameQuantityAndPrice() =>
        GetCartItems().All(item =>
            !string.IsNullOrWhiteSpace(item.FindElement(ItemName).Text) &&
            item.FindElement(ItemQuantity).Text == "1" &&
            item.FindElement(ItemPrice).Text.StartsWith("$", StringComparison.Ordinal));

    /// <summary>Clicks the Checkout button.</summary>
    public void ClickCheckout()
    {
        _js.ExecuteScript("arguments[0].click();", WaitForClickable(CheckoutButton));
        WaitForUrl("checkout-step-one");
    }

    /// <summary>Clicks the Continue Shopping button.</summary>
    public void ClickContinueShopping()
    {
        _js.ExecuteScript("arguments[0].click();", WaitForClickable(ContinueShoppingButton));
        WaitForUrl("inventory");
    }

    /// <summary>Removes the item at the given 0-based index.</summary>
    public CartPage RemoveItem(int index = 0)
    {
        var countBefore = GetItemCount();
        var buttons = Wait.Until(d =>
        {
            var availableButtons = d.FindElements(RemoveButtons);
            return availableButtons.Count > 0 ? availableButtons : null;
        })!;
        if (index >= buttons.Count)
            throw new ArgumentOutOfRangeException(nameof(index),
                $"Only {buttons.Count} remove buttons available.");

        _js.ExecuteScript("arguments[0].click();", buttons[index]);
        Wait.Until(d => d.FindElements(CartItems).Count < countBefore);
        return this;
    }

    /// <summary>Removes the first item from the cart.</summary>
    public CartPage RemoveFirstItem() => RemoveItem();

    /// <summary>Returns the current URL.</summary>
    public string GetCurrentUrl() => Driver.Url;
}
