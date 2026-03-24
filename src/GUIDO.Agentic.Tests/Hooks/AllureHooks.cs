using Allure.Net.Commons;
using TechTalk.SpecFlow;

namespace GUIDO.Agentic.Tests.Hooks;

[Binding]
public class AllureHooks
{
    private static readonly AllureLifecycle Lifecycle = AllureLifecycle.Instance;
    private static AllureContext? _featureContext;

    [BeforeFeature(Order = 0)]
    public static void BeforeFeature(FeatureContext featureContext)
    {
        Lifecycle.StartTestContainer(new TestResultContainer
        {
            name = featureContext.FeatureInfo.Title,
        });
        _featureContext = Lifecycle.Context;
    }

    [AfterFeature(Order = int.MaxValue)]
    public static void AfterFeature()
    {
        if (_featureContext == null) return;
        Lifecycle.RestoreContext(_featureContext);
        Lifecycle.StopTestContainer();
        Lifecycle.WriteTestContainer();
        _featureContext = null;
    }

    [BeforeScenario(Order = 1)]
    public void BeforeScenario(ScenarioContext scenarioContext, FeatureContext featureContext)
    {
        if (_featureContext != null)
            Lifecycle.RestoreContext(_featureContext);

        var testResult = new TestResult
        {
            name = scenarioContext.ScenarioInfo.Title,
            fullName = $"{featureContext.FeatureInfo.Title}.{scenarioContext.ScenarioInfo.Title}",
        };
        testResult.labels.Add(Label.Feature(featureContext.FeatureInfo.Title));
        testResult.labels.Add(Label.Suite(featureContext.FeatureInfo.Title));

        foreach (var tag in scenarioContext.ScenarioInfo.Tags)
        {
            testResult.labels.Add(Label.Tag(tag));
        }

        Lifecycle.StartTestCase(testResult);
        scenarioContext.Set(Lifecycle.Context, "AllureContext");
    }

    [AfterScenario(Order = int.MaxValue)]
    public void AfterScenario(ScenarioContext scenarioContext)
    {
        if (!scenarioContext.TryGetValue("AllureContext", out AllureContext? ctx) || ctx == null)
            return;

        Lifecycle.RestoreContext(ctx);
        Lifecycle.UpdateTestCase(tc =>
        {
            tc.status = scenarioContext.TestError == null ? Status.passed : Status.failed;
            if (scenarioContext.TestError != null)
            {
                tc.statusDetails = new StatusDetails
                {
                    message = scenarioContext.TestError.Message,
                    trace = scenarioContext.TestError.StackTrace,
                };
            }
        });
        Lifecycle.StopTestCase();
        Lifecycle.WriteTestCase();

        _featureContext = Lifecycle.Context;
    }

    [BeforeStep]
    public void BeforeStep(ScenarioContext scenarioContext)
    {
        if (scenarioContext.TryGetValue("AllureContext", out AllureContext? ctx) && ctx != null)
            Lifecycle.RestoreContext(ctx);

        var stepInfo = scenarioContext.StepContext.StepInfo;
        Lifecycle.StartStep(new StepResult
        {
            name = $"{stepInfo.StepDefinitionType} {stepInfo.Text}",
        });
        scenarioContext.Set(Lifecycle.Context, "AllureContext");
    }

    [AfterStep]
    public void AfterStep(ScenarioContext scenarioContext)
    {
        if (!scenarioContext.TryGetValue("AllureContext", out AllureContext? ctx) || ctx == null)
            return;

        Lifecycle.RestoreContext(ctx);
        Lifecycle.UpdateStep(s =>
        {
            s.status = scenarioContext.TestError == null ? Status.passed : Status.failed;
        });
        Lifecycle.StopStep();
        scenarioContext.Set(Lifecycle.Context, "AllureContext");
    }
}
