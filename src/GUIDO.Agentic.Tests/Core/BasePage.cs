using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace GUIDO.Agentic.Tests.Core;

/// <summary>
/// Base class for all Page Objects.
/// Provides explicit wait helpers and common navigation utilities.
/// Hard rule: never use Thread.Sleep — always use explicit waits.
/// </summary>
public abstract class BasePage
{
    protected readonly IWebDriver Driver;
    protected readonly WebDriverWait Wait;

    protected BasePage(IWebDriver driver)
    {
        Driver = driver;
        Wait = new WebDriverWait(driver, TimeSpan.FromSeconds(ConfigManager.TimeoutSeconds));
    }

    /// <summary>Waits until an element is visible and returns it.</summary>
    protected IWebElement WaitForElement(By locator)
    {
        return Wait.Until(d =>
        {
            var el = d.FindElement(locator);
            return el.Displayed ? el : null;
        })!;
    }

    /// <summary>Waits until an element is clickable and returns it.</summary>
    protected IWebElement WaitForClickable(By locator)
    {
        return Wait.Until(d =>
        {
            var el = d.FindElement(locator);
            return el.Displayed && el.Enabled ? el : null;
        })!;
    }

    /// <summary>Waits until the URL contains the expected partial path.</summary>
    protected void WaitForUrl(string partialUrl)
    {
        Wait.Until(d => d.Url.Contains(partialUrl, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>Navigates to a path relative to the configured base URL.</summary>
    protected void NavigateTo(string relativePath = "")
    {
        Driver.Navigate().GoToUrl(ConfigManager.BaseUrl.TrimEnd('/') + "/" + relativePath.TrimStart('/'));
    }

    /// <summary>Returns true when the element exists in the DOM (may be hidden).</summary>
    protected bool ElementExists(By locator)
    {
        try
        {
            Driver.FindElement(locator);
            return true;
        }
        catch (NoSuchElementException)
        {
            return false;
        }
    }
}
