using OpenQA.Selenium;

namespace GUIDO.Agentic.Tests.Core;

/// <summary>
/// Shared WebDriver instance per scenario, injected via SpecFlow DI.
/// </summary>
public class DriverContext : IDisposable
{
    public IWebDriver Driver { get; } = DriverFactory.Create();

    public void Dispose() => Driver.Quit();
}
