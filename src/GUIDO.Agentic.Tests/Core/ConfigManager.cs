using Microsoft.Extensions.Configuration;

namespace GUIDO.Agentic.Tests.Core;

/// <summary>
/// Centralised configuration manager — reads from appsettings.json.
/// Hard rule: never hardcode URLs or credentials; always use this class.
/// </summary>
public static class ConfigManager
{
    private static readonly IConfiguration s_configuration;

    static ConfigManager()
    {
        s_configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .Build();
    }

    public static string BaseUrl =>
        s_configuration["AppSettings:BaseUrl"] ?? "https://www.saucedemo.com";

    public static string StandardUser =>
        s_configuration["Credentials:StandardUser"] ?? "standard_user";

    public static string LockedOutUser =>
        s_configuration["Credentials:LockedOutUser"] ?? "locked_out_user";

    public static string ProblemUser =>
        s_configuration["Credentials:ProblemUser"] ?? "problem_user";

    public static string Password =>
        s_configuration["Credentials:Password"] ?? "secret_sauce";

    public static string Browser =>
        s_configuration["AppSettings:Browser"] ?? "chrome";

    public static bool Headless =>
        bool.TryParse(s_configuration["AppSettings:Headless"], out var result) && result;

    public static int TimeoutSeconds =>
        int.TryParse(s_configuration["AppSettings:TimeoutSeconds"], out var result)
            ? result
            : 10;
}
