using System.IO;

namespace CBoosterSharp.Playground.Util;

public class FilePathService
{
    private readonly string _basePath;

    public FilePathService()
    {
        _basePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    }

    public string GetConfigPath()
    {
        return Path.Combine(_basePath, "AppConfig", "config.json");
    }

    public string GetLogsPath()
    {
        return Path.Combine(_basePath, "AppLogs", "logs.txt");
    }

    public static string Combine(params string[] segments)
    {
        return Path.Combine(segments);
    }
}
