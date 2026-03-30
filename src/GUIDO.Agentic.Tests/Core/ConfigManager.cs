using Microsoft.Extensions.Configuration;

namespace GUIDO.Agentic.Tests.Core;

public static class ConfigManager
{
    private static readonly IConfiguration Config = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: false)
        .Build();

    public static string BaseUrl => Config["AppSettings:BaseUrl"]!;
    public static string Browser => Config["AppSettings:Browser"] ?? "chrome";
    public static bool Headless => bool.Parse(Config["AppSettings:Headless"] ?? "true");
    public static int TimeoutSeconds => int.Parse(Config["AppSettings:TimeoutSeconds"] ?? "10");

    public static string StandardUser => Config["Credentials:StandardUser"]!;
    public static string LockedOutUser => Config["Credentials:LockedOutUser"]!;
    public static string ProblemUser => Config["Credentials:ProblemUser"]!;
    public static string Password => Config["Credentials:Password"]!;
}
