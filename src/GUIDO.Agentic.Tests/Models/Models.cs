namespace GUIDO.Agentic.Tests.Models;

/// <summary>
/// Represents a SauceDemo user credential set.
/// </summary>
public record UserCredentials(string Username, string Password);

/// <summary>
/// Represents a product item as displayed on the inventory page.
/// </summary>
public record ProductItem(string Name, string Description, string Price);
