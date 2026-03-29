using BoDi;
using GUIDO.Agentic.Tests.Core;
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
    public void RegisterDriver()
    {
        _container.RegisterInstanceAs(new DriverContext());
    }

    [AfterScenario(Order = int.MaxValue)]
    public void DisposeDriver()
    {
        var ctx = _container.Resolve<DriverContext>();
        ctx.Dispose();
    }
}
