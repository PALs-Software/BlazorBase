using Newtonsoft.Json;

namespace BlazorBase.Restore;

public class AppSettings
{
    public ConnectionStrings ConnectionStrings { get; set; } = null!;
    public string FileStorePath { get; set; } = null!;
    public string TempFileStorePath { get; set; } = null!;

    public static AppSettings? LoadAppSettings(string projectPath)
    {
        var appSettingsJson = Path.Join(Path.GetDirectoryName(projectPath), "appsettings.json");
        return JsonConvert.DeserializeObject<AppSettings>(appSettingsJson);
    }
}

public class ConnectionStrings
{
    public string DefaultConnection { get; set; } = null!;
}