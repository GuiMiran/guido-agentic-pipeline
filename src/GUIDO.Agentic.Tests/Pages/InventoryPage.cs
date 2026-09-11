using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using GUIDO.Agentic.Tests.Core;

namespace GUIDO.Agentic.Tests.Pages;

/// <summary>
/// Page Object for https://www.saucedemo.com/inventory.html
/// Locators sourced from specs/inventory/inventory.context.md
/// </summary>
public class InventoryPage : BasePage
{
    private static readonly By PageTitle = By.CssSelector(".title");
    private static readonly By ProductItem = By.CssSelector(".inventory_item");
    private static readonly By ProductName = By.CssSelector(".inventory_item_name");
    private static readonly By ProductPrice = By.CssSelector(".inventory_item_price");
    private static readonly By AddToCartButton = By.CssSelector(".btn_inventory");
    private static readonly By RemoveButton = By.CssSelector(".btn_secondary.btn_inventory");
    private static readonly By SortDropdown = By.CssSelector(".product_sort_container");
    private static readonly By CartBadge = By.CssSelector(".shopping_cart_badge");

    private readonly IJavaScriptExecutor _js;

    public InventoryPage(IWebDriver driver) : base(driver)
    {
        _js = (IJavaScriptExecutor)driver;
    }

    public InventoryPage WaitForLoad()
    {
        WaitForUrl("inventory.html");
        WaitForElement(PageTitle);
        return this;
    }

    /// <summary>Returns the page title text.</summary>
    public string GetTitle() => WaitForElement(PageTitle).Text;

    public IReadOnlyList<IWebElement> GetProducts() =>
        Wait.Until(d =>
        {
            var products = d.FindElements(ProductItem);
            return products.Count > 0 ? products : null;
        })!;

    public IReadOnlyList<string> GetProductNames() =>
        GetProducts()
            .Select(p => p.FindElement(ProductName).Text)
            .ToList();

    public IReadOnlyList<decimal> GetProductPrices() =>
        GetProducts()
            .Select(p => decimal.Parse(
                p.FindElement(ProductPrice).Text.Replace("$", "")))
            .ToList();

    public void SortBy(string label)
    {
        WaitForLoad();
        var select = new SelectElement(WaitForElement(SortDropdown));
        select.SelectByText(label);
    }

    public void AddFirstProductToCart()
    {
        WaitForLoad();
        var button = Wait.Until(d =>
            d.FindElements(AddToCartButton).FirstOrDefault(b => b.Text == "Add to cart"));
        _js.ExecuteScript("arguments[0].click();", button!);
        Wait.Until(d => d.FindElements(CartBadge).Count > 0);
    }

    public void RemoveFirstProductFromCart()
    {
        var countBefore = Driver.FindElements(CartBadge).Count;
        _js.ExecuteScript("arguments[0].click();", WaitForClickable(RemoveButton));
        Wait.Until(d => d.FindElements(CartBadge).Count < countBefore);
    }

    public string GetCartBadgeText() =>
        WaitForElement(CartBadge).Text;

    public bool IsCartBadgeVisible()
    {
        try
        {
            return Driver.FindElement(CartBadge).Displayed;
        }
        catch (NoSuchElementException)
        {
            return false;
        }
    }

    public bool EachProductHasNamePriceAndButton() =>
        GetProducts().All(p =>
            !string.IsNullOrWhiteSpace(p.FindElement(ProductName).Text) &&
            p.FindElement(ProductPrice).Text.StartsWith("$") &&
            p.FindElements(AddToCartButton).Count > 0);
}
