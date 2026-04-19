using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using GUIDO.Agentic.Tests.Core;

namespace GUIDO.Agentic.Tests.Pages;

/// <summary>
/// Page Object for https://www.saucedemo.com/inventory.html
/// Locators sourced from specs/inventory/inventory.context.md
/// </summary>
public class InventoryPage
{
    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;

    public InventoryPage(IWebDriver driver)
    {
        _driver = driver;
        _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(ConfigManager.TimeoutSeconds));
    }

    public InventoryPage(IWebDriver driver)
    {
        _wait.Until(d => d.Url.Contains("inventory.html"));
        _wait.Until(d => d.FindElement(By.CssSelector(".title")));
        return this;
    }

    /// <summary>Returns the page title text.</summary>
    public string GetTitle() =>
        _wait.Until(d => d.FindElement(By.CssSelector(".title"))).Text;

    public IReadOnlyList<IWebElement> GetProducts() =>
        _wait.Until(d => d.FindElements(By.CssSelector(".inventory_item")));

    public IReadOnlyList<string> GetProductNames() =>
        GetProducts()
            .Select(p => p.FindElement(By.CssSelector(".inventory_item_name")).Text)
            .ToList();

    public IReadOnlyList<decimal> GetProductPrices() =>
        GetProducts()
            .Select(p => decimal.Parse(
                p.FindElement(By.CssSelector(".inventory_item_price")).Text.Replace("$", "")))
            .ToList();

    public void SortBy(string label)
    {
        var select = new SelectElement(_driver.FindElement(By.CssSelector(".product_sort_container")));
        select.SelectByText(label);
    }

    public void AddFirstProductToCart()
    {
        var buttons = _driver.FindElements(By.CssSelector(".btn_inventory"));
        buttons.First(b => b.Text == "Add to cart").Click();
    }

    public void RemoveFirstProductFromCart()
    {
        int countBefore = _driver.FindElements(By.CssSelector(".shopping_cart_badge")).Count;
        _driver.FindElement(By.CssSelector(".btn_secondary.btn_inventory")).Click();
        _wait.Until(d => d.FindElements(By.CssSelector(".shopping_cart_badge")).Count < countBefore);
    }

    public string GetCartBadgeText() =>
        _driver.FindElement(By.CssSelector(".shopping_cart_badge")).Text;

    public bool IsCartBadgeVisible()
    {
        try
        {
            return _driver.FindElement(By.CssSelector(".shopping_cart_badge")).Displayed;
        }
        catch (NoSuchElementException)
        {
            return false;
        }
    }

    public bool EachProductHasNamePriceAndButton() =>
        GetProducts().All(p =>
            !string.IsNullOrWhiteSpace(p.FindElement(By.CssSelector(".inventory_item_name")).Text) &&
            p.FindElement(By.CssSelector(".inventory_item_price")).Text.StartsWith("$") &&
            p.FindElements(By.CssSelector(".btn_inventory")).Count > 0);
}
