using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace GUIDO.Agentic.Tests.Core;

public static class DriverFactory
{
    public static IWebDriver Create()
    {
        var options = new ChromeOptions();
        if (ConfigManager.Headless)
        {
            options.AddArgument("--headless=new");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
        }

        // Selenium 4.x selenium-manager resolves the matching driver automatically
        return new ChromeDriver(options);
    }
}
