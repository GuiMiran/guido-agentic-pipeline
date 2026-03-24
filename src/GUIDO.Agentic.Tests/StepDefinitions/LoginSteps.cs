using GUIDO.Agentic.Tests.Core;
using GUIDO.Agentic.Tests.Pages;
using FluentAssertions;
using TechTalk.SpecFlow;

namespace GUIDO.Agentic.Tests.StepDefinitions;

[Binding]
public class LoginSteps : IDisposable
{
    private readonly IWebDriverContext _context;
    private readonly LoginPage _loginPage;
    private readonly InventoryPage _inventoryPage;

    public LoginSteps(IWebDriverContext context)
    {
        _context = context;
        _loginPage = new LoginPage(_context.Driver);
        _inventoryPage = new InventoryPage(_context.Driver);
    }

    [Given(@"I am on the SauceDemo login page")]
    public void GivenIAmOnTheSauceDemoLoginPage()
    {
        _loginPage.Open();
    }

    [When(@"I enter username ""(.*)"" and password ""(.*)""")]
    public void WhenIEnterUsernameAndPassword(string username, string password)
    {
        if (!string.IsNullOrEmpty(username))
            _loginPage.EnterUsername(username);

        if (!string.IsNullOrEmpty(password))
            _loginPage.EnterPassword(password);
    }

    [When(@"I click the login button")]
    public void WhenIClickTheLoginButton()
    {
        _loginPage.ClickLogin();
    }

    [Then(@"I should be redirected to the inventory page")]
    public void ThenIShouldBeRedirectedToTheInventoryPage()
    {
        _inventoryPage.WaitForLoad();
        _context.Driver.Url.Should().Contain("inventory.html");
    }

    [Then(@"the page title should be ""(.*)""")]
    public void ThenThePageTitleShouldBe(string expectedTitle)
    {
        _inventoryPage.GetTitle().Should().Be(expectedTitle);
    }

    [Then(@"I should see an error message containing ""(.*)""")]
    public void ThenIShouldSeeAnErrorMessageContaining(string expectedText)
    {
        _loginPage.HasErrorMessage().Should().BeTrue("an error message should be displayed");
        _loginPage.GetErrorMessage().Should().ContainEquivalentOf(expectedText);
    }

    public void Dispose() { }
}
