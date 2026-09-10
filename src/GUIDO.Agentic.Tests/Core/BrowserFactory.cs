using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;

namespace GUIDO.Agentic.Tests.Core;

/// <summary>
/// Factory responsible for creating and configuring Selenium WebDriver instances.
/// Uses Selenium's built-in driver management when creating browser drivers.
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
        var options = new ChromeOptions();

        if (ConfigManager.Headless)
        {
            options.AddArgument("--headless=new");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--window-size=1920,1080");
        }

        var service = ResolveChromeDriverService();
        return service == null
            ? new ChromeDriver(options)
            : new ChromeDriver(service, options);
    }

    private static IWebDriver CreateFirefox()
    {
        var options = new FirefoxOptions();

        if (ConfigManager.Headless)
        {
            options.AddArgument("--headless");
            options.AddArgument("--width=1920");
            options.AddArgument("--height=1080");
        }

        return new FirefoxDriver(options);
    }

    private static ChromeDriverService? ResolveChromeDriverService()
    {
        var path = FindOnPath("chromedriver", "chromedriver.exe");
        return path == null
            ? null
            : ChromeDriverService.CreateDefaultService(
                Path.GetDirectoryName(path)!,
                Path.GetFileName(path));
    }

    private static string? FindOnPath(params string[] fileNames)
    {
        var pathValue = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrWhiteSpace(pathValue))
            return null;

        foreach (var directory in pathValue.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            foreach (var fileName in fileNames)
            {
                var candidate = Path.Combine(directory, fileName);
                if (File.Exists(candidate))
                    return candidate;
            }
        }

        return null;
    }
}
