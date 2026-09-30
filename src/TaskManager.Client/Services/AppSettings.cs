using System.Configuration;

namespace TaskManager.Client.Services;

internal static class AppSettings
{
    public const string ServerUrlKey = "ServerUrl";
    public const string DefaultServerUrl = "http://localhost:5080";

    public static string GetServerUrl()
    {
        var url = ConfigurationManager.AppSettings[ServerUrlKey];
        return string.IsNullOrWhiteSpace(url) ? DefaultServerUrl : url.Trim();
    }

    public static void SaveServerUrl(string url)
    {
        var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        var settings = config.AppSettings.Settings;
        if (settings[ServerUrlKey] is null)
        {
            settings.Add(ServerUrlKey, url);
        }
        else
        {
            settings[ServerUrlKey].Value = url;
        }

        config.Save(ConfigurationSaveMode.Modified);
        ConfigurationManager.RefreshSection("appSettings");
    }
}
