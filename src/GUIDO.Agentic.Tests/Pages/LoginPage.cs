using GUIDO.Agentic.Tests.Core;
using OpenQA.Selenium;

namespace GUIDO.Agentic.Tests.Pages;

/// <summary>
/// Page Object for the SauceDemo login page (/).
/// All locators are sourced from specs/auth/login.context.md.
/// </summary>
public class LoginPage : BasePage
{
    // Locators — sourced from login.context.md
    private static readonly By UsernameField = By.Id("user-name");
    private static readonly By PasswordField = By.Id("password");
    private static readonly By LoginButton = By.Id("login-button");
    private static readonly By ErrorMessage = By.CssSelector("[data-test='error']");

    public LoginPage(IWebDriver driver) : base(driver) { }

    /// <summary>Navigates to the SauceDemo login page.</summary>
    public LoginPage Open()
    {
        NavigateTo();
        return this;
    }

    /// <summary>Enters a username into the username field.</summary>
    public LoginPage EnterUsername(string username)
    {
        var field = WaitForElement(UsernameField);
        field.Clear();
        field.SendKeys(username);
        return this;
    }

    /// <summary>Enters a password into the password field.</summary>
    public LoginPage EnterPassword(string password)
    {
        var field = WaitForElement(PasswordField);
        field.Clear();
        field.SendKeys(password);
        return this;
    }

    /// <summary>Clicks the Login button.</summary>
    public void ClickLogin()
    {
        WaitForClickable(LoginButton).Click();
    }

    /// <summary>Performs a full login flow with the supplied credentials.</summary>
    public void Login(string username, string password)
    {
        EnterUsername(username);
        EnterPassword(password);
        ClickLogin();
    }

    /// <summary>Returns the visible error message text, or empty string if none.</summary>
    public string GetErrorMessage()
    {
        return ElementExists(ErrorMessage)
            ? Driver.FindElement(ErrorMessage).Text
            : string.Empty;
    }

    /// <summary>Returns true when the error message container is displayed.</summary>
    public bool HasErrorMessage() => ElementExists(ErrorMessage);
}
