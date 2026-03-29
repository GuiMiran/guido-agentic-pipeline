using FluentAssertions;
using GUIDO.Agentic.Tests.Core;
using GUIDO.Agentic.Tests.Pages;
using TechTalk.SpecFlow;

namespace GUIDO.Agentic.Tests.StepDefinitions;

[Binding]
public class AuthSteps
{
    private readonly LoginPage _loginPage;

    public AuthSteps(DriverContext ctx)
    {
        _loginPage = new LoginPage(ctx.Driver);
    }

    [Given(@"I am on the SauceDemo login page")]
    public void GivenIAmOnTheSauceDemoLoginPage() => _loginPage.Navigate();

    [When(@"I enter username ""(.*)"" and password ""(.*)""")]
    public void WhenIEnterUsernameAndPassword(string username, string password)
    {
        if (!string.IsNullOrEmpty(username)) _loginPage.EnterUsername(username);
        if (!string.IsNullOrEmpty(password)) _loginPage.EnterPassword(password);
    }

    [When(@"I click the login button")]
    public void WhenIClickTheLoginButton() => _loginPage.ClickLogin();

    [Then(@"I should be redirected to the inventory page")]
    public void ThenIShouldBeRedirectedToTheInventoryPage() =>
        _loginPage.GetCurrentUrl().Should().Contain("inventory");

    [Then(@"the page title should be ""(.*)""")]
    public void ThenThePageTitleShouldBe(string expectedTitle) =>
        _loginPage.GetPageTitle().Should().Be(expectedTitle);

    [Then(@"I should see an error message containing ""(.*)""")]
    public void ThenIShouldSeeAnErrorMessageContaining(string expectedText) =>
        _loginPage.GetErrorMessage().Should().Contain(expectedText);
}
