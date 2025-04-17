using Microsoft.Extensions.Configuration;

namespace BlazorBase.Restore;

public class AppSettings
{
    public ConnectionStrings? ConnectionStrings { get; set; } = null;
    public string? FileStorePath { get; set; } = null;
    public string? TempFileStorePath { get; set; } = null;

    public static AppSettings? LoadAppSettings(string projectPath)
    {
        var basePath = Path.GetDirectoryName(projectPath);
        if (String.IsNullOrEmpty(basePath))
            return null;

        var configurationBuilder = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile("appsettings.User.json", optional: true, reloadOnChange: false);

        return configurationBuilder.Build().Get<AppSettings?>();
    }
}

public class ConnectionStrings
{
    public string? DefaultConnection { get; set; } = null!;
}