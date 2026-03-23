using OpenQA.Selenium;

namespace GUIDO.Agentic.Tests.Core;

/// <summary>
/// Shared context that holds the WebDriver instance for a single SpecFlow scenario.
/// Injected via SpecFlow's built-in IoC container (BoDi).
/// </summary>
public interface IWebDriverContext
{
    IWebDriver Driver { get; }
}

/// <summary>
/// Default implementation that creates and owns a WebDriver for one scenario.
/// </summary>
public class WebDriverContext : IWebDriverContext, IDisposable
{
    private bool _disposed;

    public IWebDriver Driver { get; }

    public WebDriverContext()
    {
        Driver = BrowserFactory.Create();
        Driver.Manage().Window.Maximize();
        Driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);
        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.Zero;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            Driver?.Quit();
            Driver?.Dispose();
        }

        _disposed = true;
    }
}
