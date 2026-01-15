using System;
using System.IO;

namespace Filebuloso.Helpers;

public static class AppPaths
{
    public static string AppDataRoot =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Filebuloso");

    public static string ConfigPath => Path.Combine(AppDataRoot, "config.json");

    public static string DocumentsRoot =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Filebuloso");

    public static string LogsRoot => Path.Combine(DocumentsRoot, "Logs");

    public static string ErrorLogsRoot => Path.Combine(LogsRoot, "Errors");

    public static void EnsureDirectories()
    {
        Directory.CreateDirectory(AppDataRoot);
        Directory.CreateDirectory(LogsRoot);
        Directory.CreateDirectory(ErrorLogsRoot);
    }
}
