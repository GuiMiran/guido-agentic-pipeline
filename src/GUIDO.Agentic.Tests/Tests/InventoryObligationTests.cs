using FluentAssertions;
using GUIDO.Agentic.Tests.Core;
using GUIDO.Agentic.Tests.Helpers;
using GUIDO.Agentic.Tests.Pages;
using Xunit;

namespace GUIDO.Agentic.Tests.Tests;

public class InventoryObligationTests : BaseTest
{
    [Fact(DisplayName = "Standard user inventory contains exactly six products"), Trait("Spec", "INVENTORY-008")]
    public void ExactProductCount()
    {
        LoginHelper.LoginAsStandardUser(Driver);
        var inventory = new InventoryPage(Driver).WaitForLoad();

        inventory.GetProducts().Count.Should().Be(6);
    }

    [Fact(DisplayName = "Every inventory product displays a non-empty description"), Trait("Spec", "INVENTORY-009")]
    public void ProductDescriptions()
    {
        LoginHelper.LoginAsStandardUser(Driver);
        var inventory = new InventoryPage(Driver).WaitForLoad();
        var descriptions = inventory.GetProductDescriptions();

        descriptions.Should().HaveCount(6);
        foreach (var productDescriptions in descriptions)
        {
            productDescriptions.Should().ContainSingle();
            productDescriptions[0].Visible.Should().BeTrue();
            productDescriptions[0].Text.Should().NotBeNullOrWhiteSpace();
        }
    }
}
