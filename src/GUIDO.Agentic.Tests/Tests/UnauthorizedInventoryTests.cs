using FluentAssertions;
using GUIDO.Agentic.Tests.Core;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Xunit;

namespace GUIDO.Agentic.Tests.Tests;

public class UnauthorizedInventoryTests : BaseTest
{
    [Fact(DisplayName = "Unauthenticated inventory access returns to login"), Trait("Spec", "AUTH-006")]
    public void DeniesUnauthenticatedInventory()
    {
        // BaseTest supplies a new browser/session; deliberately never log in.
        var loginUrl = ConfigManager.BaseUrl.TrimEnd('/') + "/";
        Driver.Navigate().GoToUrl(loginUrl + "inventory.html");

        var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(ConfigManager.TimeoutSeconds));
        wait.Until(driver => driver.Url == loginUrl);
        wait.Until(driver => driver.FindElement(By.Id("login-button")).Displayed);

        Driver.Url.Should().Be(loginUrl);
        Driver.FindElement(By.Id("user-name")).Displayed.Should().BeTrue();
        Driver.FindElement(By.Id("password")).Displayed.Should().BeTrue();
        Driver.FindElements(By.CssSelector(".inventory_list")).Should().BeEmpty();
    }
}
