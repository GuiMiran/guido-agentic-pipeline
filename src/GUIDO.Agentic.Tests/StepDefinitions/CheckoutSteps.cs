using FluentAssertions;
using GUIDO.Agentic.Tests.Core;
using GUIDO.Agentic.Tests.Pages;
using TechTalk.SpecFlow;

namespace GUIDO.Agentic.Tests.StepDefinitions;

[Binding]
public class CheckoutSteps
{
    private readonly CheckoutPage _checkoutPage;

    public CheckoutSteps(IWebDriverContext context)
    {
        _checkoutPage = new CheckoutPage(context.Driver);
    }

    [When(@"I fill in checkout info with first name ""(.*)"", last name ""(.*)"", and zip ""(.*)""")]
    public void WhenIFillInCheckoutInfo(string firstName, string lastName, string zip)
    {
        _checkoutPage
            .WaitForStepOne()
            .EnterFirstName(firstName)
            .EnterLastName(lastName)
            .EnterZip(zip);
    }

    [When(@"I click continue on checkout")]
    public void WhenIClickContinueOnCheckout() => _checkoutPage.ClickContinue();

    [When(@"I click finish")]
    public void WhenIClickFinish() => _checkoutPage.ClickFinish();

    [When(@"I click back home")]
    public void WhenIClickBackHome() => _checkoutPage.ClickBackHome();

    [Then(@"I should be on the checkout overview page")]
    public void ThenIShouldBeOnCheckoutOverviewPage() =>
        _checkoutPage.GetCurrentUrl().Should().Contain("checkout-step-two");

    [Then(@"I should be on the checkout complete page")]
    public void ThenIShouldBeOnCheckoutCompletePage() =>
        _checkoutPage.GetCurrentUrl().Should().Contain("checkout-complete");

    [Then(@"the confirmation header should contain ""(.*)""")]
    public void ThenConfirmationHeaderShouldContain(string expected) =>
        _checkoutPage.GetCompleteHeader().Should().Contain(expected);

    [Then(@"I should see a checkout error containing ""(.*)""")]
    public void ThenIShouldSeeACheckoutErrorContaining(string expected) =>
        _checkoutPage.GetErrorMessage().Should().Contain(expected);

    [Then(@"I should see at least 1 item in the checkout overview")]
    public void ThenIShouldSeeAtLeastOneItemInOverview() =>
        _checkoutPage.HasOverviewItems().Should().BeTrue();

    [Then(@"the checkout overview should display subtotal, tax, and total")]
    public void ThenCheckoutOverviewShouldDisplaySummary() =>
        _checkoutPage.HasOrderSummary().Should().BeTrue();
}
