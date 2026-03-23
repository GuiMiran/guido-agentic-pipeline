using System.Text.Json;
using GUIDO.Agentic.Tests.Models;

namespace GUIDO.Agentic.Tests.Helpers;

/// <summary>
/// Reads test data from JSON files located in the TestData directory.
/// </summary>
public static class TestDataLoader
{
    private static readonly string TestDataPath =
        Path.Combine(AppContext.BaseDirectory, "TestData");

    /// <summary>
    /// Loads and deserialises a JSON file from the TestData folder.
    /// </summary>
    public static T Load<T>(string fileName)
    {
        var path = Path.Combine(TestDataPath, fileName);

        if (!File.Exists(path))
            throw new FileNotFoundException($"Test data file not found: {path}");

        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<T>(json)
               ?? throw new InvalidOperationException($"Failed to deserialise {fileName}");
    }

    /// <summary>
    /// Loads user credentials from the standard login data file.
    /// </summary>
    public static IEnumerable<UserCredentials> LoadLoginCredentials()
    {
        return Load<List<UserCredentials>>("login.data.json");
    }
}
