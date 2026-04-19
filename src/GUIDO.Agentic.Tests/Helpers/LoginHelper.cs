using OpenQA.Selenium;
using GUIDO.Agentic.Tests.Core;
using GUIDO.Agentic.Tests.Pages;

namespace GUIDO.Agentic.Tests.Helpers;

public static class LoginHelper
{
    public static void LoginAsStandardUser(IWebDriver driver)
    {
        var page = new LoginPage(driver);
        page.Open();
        page.EnterUsername(ConfigManager.StandardUser);
        page.EnterPassword(ConfigManager.Password);
        page.ClickLogin();
    }
}
