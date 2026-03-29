using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using GUIDO.Agentic.Tests.Core;

namespace GUIDO.Agentic.Tests.Pages;

/// <summary>
/// Page Object for https://www.saucedemo.com
/// Locators sourced from specs/auth/login.context.md
/// </summary>
public class LoginPage
{
    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;

    private IWebElement UsernameField => _driver.FindElement(By.Id("user-name"));
    private IWebElement PasswordField => _driver.FindElement(By.Id("password"));
    private IWebElement LoginButton => _driver.FindElement(By.Id("login-button"));
    private IWebElement ErrorMessage => _wait.Until(d => d.FindElement(By.CssSelector("[data-test='error']")));

    public LoginPage(IWebDriver driver)
    {
        _driver = driver;
        _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(ConfigManager.TimeoutSeconds));
    }

    public void Navigate() => _driver.Navigate().GoToUrl(ConfigManager.BaseUrl);

    public void EnterUsername(string username) => UsernameField.SendKeys(username);

    public void EnterPassword(string password) => PasswordField.SendKeys(password);

    public void ClickLogin() => LoginButton.Click();

    public string GetErrorMessage() => ErrorMessage.Text;

    public string GetCurrentUrl() => _driver.Url;

    public string GetPageTitle() =>
        _wait.Until(d => d.FindElement(By.CssSelector(".title"))).Text;
}
