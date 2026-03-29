using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using GUIDO.Agentic.Tests.Core;

namespace GUIDO.Agentic.Tests.Pages;

/// <summary>
/// Page Object for https://www.saucedemo.com/cart.html
/// Locators sourced from specs/cart/cart.context.md
/// </summary>
public class CartPage
{
    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;
    private readonly IJavaScriptExecutor _js;

    public CartPage(IWebDriver driver)
    {
        _driver = driver;
        _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(ConfigManager.TimeoutSeconds));
        _js = (IJavaScriptExecutor)driver;
    }

    public void Navigate()
    {
        _driver.Navigate().GoToUrl($"{ConfigManager.BaseUrl}/cart.html");
        _wait.Until(d => d.FindElement(By.CssSelector(".cart_list")));
    }

    public IReadOnlyList<IWebElement> GetCartItems() =>
        _driver.FindElements(By.CssSelector(".cart_item"));

    public int GetCartItemCount() => GetCartItems().Count;

    public bool IsCartEmpty() =>
        _wait.Until(d => d.FindElements(By.CssSelector(".cart_item")).Count == 0);

    public bool EachItemHasNameQuantityAndPrice() =>
        GetCartItems().All(item =>
            !string.IsNullOrWhiteSpace(item.FindElement(By.CssSelector(".inventory_item_name")).Text) &&
            !string.IsNullOrWhiteSpace(item.FindElement(By.CssSelector(".cart_quantity")).Text) &&
            item.FindElement(By.CssSelector(".inventory_item_price")).Text.StartsWith("$"));

    public void RemoveFirstItem()
    {
        int countBefore = _driver.FindElements(By.CssSelector(".cart_item")).Count;
        var btn = _driver.FindElement(By.CssSelector(".cart_item button"));
        _js.ExecuteScript("arguments[0].click();", btn);
        _wait.Until(d => d.FindElements(By.CssSelector(".cart_item")).Count < countBefore);
    }

    public void ClickContinueShopping()
    {
        var btn = _driver.FindElement(By.Id("continue-shopping"));
        _js.ExecuteScript("arguments[0].click();", btn);
        _wait.Until(d => !d.Url.Contains("cart.html"));
    }

    public void ClickCheckout()
    {
        var btn = _driver.FindElement(By.Id("checkout"));
        _js.ExecuteScript("arguments[0].click();", btn);
        _wait.Until(d => !d.Url.Contains("cart.html"));
    }

    public string GetCurrentUrl() => _driver.Url;
}
