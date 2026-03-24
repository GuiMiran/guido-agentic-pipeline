using GUIDO.Agentic.Tests.Core;
using OpenQA.Selenium;

namespace GUIDO.Agentic.Tests.Pages;

/// <summary>
/// Page Object for the SauceDemo inventory page (/inventory.html).
/// All locators are sourced from specs/inventory/inventory.context.md.
/// </summary>
public class InventoryPage : BasePage
{
    // Locators
    private static readonly By PageTitle = By.CssSelector(".title");
    private static readonly By InventoryItems = By.CssSelector(".inventory_item");
    private static readonly By CartBadge = By.CssSelector(".shopping_cart_badge");
    private static readonly By CartLink = By.CssSelector(".shopping_cart_link");
    private static readonly By SortDropdown = By.CssSelector("[data-test='product-sort-container']");
    private static readonly By BurgerMenu = By.Id("react-burger-menu-btn");
    private static readonly By LogoutLink = By.Id("logout_sidebar_link");

    public InventoryPage(IWebDriver driver) : base(driver) { }

    /// <summary>Waits until the inventory page has fully loaded.</summary>
    public InventoryPage WaitForLoad()
    {
        WaitForUrl("inventory.html");
        WaitForElement(PageTitle);
        return this;
    }

    /// <summary>Returns the page title text.</summary>
    public string GetTitle() => WaitForElement(PageTitle).Text;

    /// <summary>Returns all inventory item elements.</summary>
    public IReadOnlyCollection<IWebElement> GetItems() =>
        Driver.FindElements(InventoryItems);

    /// <summary>Returns the number of products displayed.</summary>
    public int GetItemCount() => GetItems().Count;

    /// <summary>Returns the cart badge count, or 0 if the badge is not visible.</summary>
    public int GetCartBadgeCount()
    {
        if (!ElementExists(CartBadge)) return 0;
        return int.TryParse(Driver.FindElement(CartBadge).Text, out var count) ? count : 0;
    }

    /// <summary>Clicks "Add to cart" for the item at the given 0-based index.</summary>
    public InventoryPage AddItemToCart(int index = 0)
    {
        var addButtons = Driver.FindElements(By.CssSelector(".btn_inventory"));
        if (index >= addButtons.Count)
            throw new ArgumentOutOfRangeException(nameof(index),
                $"Only {addButtons.Count} items available.");

        addButtons[index].Click();
        return this;
    }

    /// <summary>Clicks the shopping cart icon to navigate to the cart.</summary>
    public void GoToCart() => WaitForClickable(CartLink).Click();

    /// <summary>Logs out via the burger menu.</summary>
    public void Logout()
    {
        WaitForClickable(BurgerMenu).Click();
        WaitForClickable(LogoutLink).Click();
    }
}
