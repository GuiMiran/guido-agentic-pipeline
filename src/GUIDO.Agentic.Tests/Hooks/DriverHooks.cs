using GUIDO.Agentic.Tests.Core;
using BoDi;
using TechTalk.SpecFlow;

namespace GUIDO.Agentic.Tests.Hooks;

[Binding]
public class DriverHooks
{
    private readonly IObjectContainer _container;

    public DriverHooks(IObjectContainer container)
    {
        _container = container;
    }

    [BeforeScenario(Order = 0)]
    public void BeforeScenario()
    {
        var context = new WebDriverContext();
        _container.RegisterInstanceAs<IWebDriverContext>(context);
    }

    [AfterScenario]
    public void AfterScenario()
    {
        var context = _container.Resolve<IWebDriverContext>();
        (context as IDisposable)?.Dispose();
    }
}
