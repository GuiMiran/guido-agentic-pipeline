using OpenQA.Selenium;

namespace GUIDO.Agentic.Tests.Core;

/// <summary>
/// Base class for all test fixtures.
/// Manages WebDriver lifecycle (creation before each scenario, disposal after).
/// </summary>
public abstract class BaseTest : IDisposable
{
    protected IWebDriver Driver { get; private set; }
    private bool _disposed;

    protected BaseTest()
    {
        Driver = BrowserFactory.Create();
        Driver.Manage().Window.Maximize();
        Driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);
        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.Zero; // rely on explicit waits
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
