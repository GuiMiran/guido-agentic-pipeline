using OpenQA.Selenium;
using GUIDO.Agentic.Tests.Core;

namespace GUIDO.Agentic.Tests.Pages;

/// <summary>
/// Page Object for the SauceDemo checkout flow (3 steps).
/// All locators are sourced from specs/checkout/checkout.context.md.
/// </summary>
public class CheckoutPage : BasePage
{
    // Locators — Step 1 (Info)
    private static readonly By FirstNameField  = By.CssSelector("[data-test='firstName']");
    private static readonly By LastNameField   = By.CssSelector("[data-test='lastName']");
    private static readonly By ZipField        = By.CssSelector("[data-test='postalCode']");
    private static readonly By ContinueButton  = By.CssSelector("[data-test='continue']");
    private static readonly By CancelButton    = By.CssSelector("[data-test='cancel']");
    private static readonly By ErrorMessage    = By.CssSelector("[data-test='error']");

    // Locators — Step 2 (Overview)
    private static readonly By OverviewItems     = By.CssSelector(".cart_item");
    private static readonly By SubtotalLabel     = By.CssSelector(".summary_subtotal_label");
    private static readonly By TaxLabel          = By.CssSelector(".summary_tax_label");
    private static readonly By TotalLabel        = By.CssSelector(".summary_total_label");
    private static readonly By FinishButton      = By.CssSelector("[data-test='finish']");

    // Locators — Step 3 (Complete)
    private static readonly By CompleteHeader    = By.CssSelector(".complete-header");
    private static readonly By BackHomeButton    = By.CssSelector("[data-test='back-to-products']");

    private readonly IJavaScriptExecutor _js;

    public CheckoutPage(IWebDriver driver) : base(driver)
    {
        _js = (IJavaScriptExecutor)driver;
    }

    /// <summary>Waits until checkout step one has loaded.</summary>
    public CheckoutPage WaitForStepOne()
    {
        WaitForUrl("checkout-step-one");
        WaitForElement(FirstNameField);
        return this;
    }

    /// <summary>
    /// Fills a React-controlled input by using the native value setter.
    /// Required because React's synthetic onChange is not triggered by plain SendKeys.
    /// </summary>
    private void SetReactField(By locator, string value)
    {
        var field = WaitForElement(locator);
        _js.ExecuteScript(
            "var s = Object.getOwnPropertyDescriptor(window.HTMLInputElement.prototype, 'value').set;" +
            "s.call(arguments[0], arguments[1]);" +
            "arguments[0].dispatchEvent(new Event('input', {bubbles:true}));",
            field, value);
    }

    /// <summary>Enters the first name. Skipped when value is empty (validates empty-field error).</summary>
    public CheckoutPage EnterFirstName(string value)
    {
        if (!string.IsNullOrEmpty(value)) SetReactField(FirstNameField, value);
        return this;
    }

    /// <summary>Enters the last name. Skipped when value is empty.</summary>
    public CheckoutPage EnterLastName(string value)
    {
        if (!string.IsNullOrEmpty(value)) SetReactField(LastNameField, value);
        return this;
    }

    /// <summary>Enters the zip/postal code. Skipped when value is empty.</summary>
    public CheckoutPage EnterZip(string value)
    {
        if (!string.IsNullOrEmpty(value)) SetReactField(ZipField, value);
        return this;
    }

    /// <summary>Clicks the Continue button. May navigate to step two or show a validation error.</summary>
    public void ClickContinue()
    {
        _js.ExecuteScript("arguments[0].click();", WaitForClickable(ContinueButton));
    }

    /// <summary>Clicks the Finish button and waits for the order confirmation page.</summary>
    public void ClickFinish()
    {
        _js.ExecuteScript("arguments[0].click();", WaitForClickable(FinishButton));
        WaitForUrl("checkout-complete");
    }

    /// <summary>Clicks Back Home and waits for the inventory page.</summary>
    public void ClickBackHome()
    {
        _js.ExecuteScript("arguments[0].click();", WaitForClickable(BackHomeButton));
        WaitForUrl("inventory");
    }

    /// <summary>Returns the validation error message text.</summary>
    public string GetErrorMessage() => WaitForElement(ErrorMessage).Text;

    /// <summary>Returns the current URL.</summary>
    public string GetCurrentUrl() => Driver.Url;

    /// <summary>Returns all order overview items.</summary>
    public IReadOnlyCollection<IWebElement> GetOverviewItems() =>
        Driver.FindElements(OverviewItems);

    /// <summary>Returns true when at least one item is shown in the overview.</summary>
    public bool HasOverviewItems() => GetOverviewItems().Count > 0;

    /// <summary>Returns true when subtotal, tax, and total labels are all present.</summary>
    public bool HasOrderSummary() =>
        ElementExists(SubtotalLabel) &&
        ElementExists(TaxLabel) &&
        ElementExists(TotalLabel);

    /// <summary>Returns the confirmation header text.</summary>
    public string GetCompleteHeader() => WaitForElement(CompleteHeader).Text;
}
