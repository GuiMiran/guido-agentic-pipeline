using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

namespace GUIDO.Agentic.Tests.Core;

/// <summary>
/// Factory responsible for creating and configuring Selenium WebDriver instances.
/// Uses WebDriverManager for automatic driver binary management.
/// </summary>
public static class BrowserFactory
{
    /// <summary>
    /// Creates a WebDriver instance based on the configured browser.
    /// Headless mode is controlled via <see cref="ConfigManager.Headless"/>.
    /// </summary>
    public static IWebDriver Create()
    {
        return ConfigManager.Browser.ToLowerInvariant() switch
        {
            "firefox" => CreateFirefox(),
            _ => CreateChrome()
        };
    }

    private static IWebDriver CreateChrome()
    {
        new DriverManager().SetUpDriver(new ChromeConfig());

        var options = new ChromeOptions();

        if (ConfigManager.Headless)
        {
            options.AddArgument("--headless=new");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--window-size=1920,1080");
        }

        return new ChromeDriver(options);
    }

    private static IWebDriver CreateFirefox()
    {
        new DriverManager().SetUpDriver(new FirefoxConfig());

        var options = new FirefoxOptions();

        if (ConfigManager.Headless)
        {
            options.AddArgument("--headless");
            options.AddArgument("--width=1920");
            options.AddArgument("--height=1080");
        }

        return new FirefoxDriver(options);
    }
}
