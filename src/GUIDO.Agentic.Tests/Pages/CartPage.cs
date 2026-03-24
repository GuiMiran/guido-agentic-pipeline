using GUIDO.Agentic.Tests.Core;
using OpenQA.Selenium;

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
    private static readonly By CheckoutButton = By.Id("checkout");
    private static readonly By ContinueShoppingButton = By.Id("continue-shopping");
    private static readonly By RemoveButtons = By.CssSelector(".cart_button");

    public CartPage(IWebDriver driver) : base(driver) { }

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

    /// <summary>Returns true when the cart contains at least one item.</summary>
    public bool HasItems() => GetItemCount() > 0;

    /// <summary>Clicks the Checkout button.</summary>
    public void ClickCheckout() => WaitForClickable(CheckoutButton).Click();

    /// <summary>Clicks the Continue Shopping button.</summary>
    public void ClickContinueShopping() => WaitForClickable(ContinueShoppingButton).Click();

    /// <summary>Removes the item at the given 0-based index.</summary>
    public CartPage RemoveItem(int index = 0)
    {
        var buttons = Driver.FindElements(RemoveButtons);
        if (index >= buttons.Count)
            throw new ArgumentOutOfRangeException(nameof(index),
                $"Only {buttons.Count} remove buttons available.");

        buttons[index].Click();
        return this;
    }
}
