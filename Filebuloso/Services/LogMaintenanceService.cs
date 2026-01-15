using System;
using System.IO;
using System.Linq;
using Filebuloso.Helpers;
using Filebuloso.Models;

namespace Filebuloso.Services;

public sealed class LogMaintenanceService
{
    public void CleanupLogs(LoggingSettings settings)
    {
        CleanupDirectory(AppPaths.LogsRoot, settings.KeepDays, settings.MaxLogFiles);
        CleanupDirectory(AppPaths.ErrorLogsRoot, settings.ErrorLogDays, int.MaxValue);
    }

    private static void CleanupDirectory(string path, int keepDays, int maxFiles)
    {
        if (!Directory.Exists(path))
        {
            return;
        }

        var cutoff = DateTime.Now.AddDays(-keepDays);
        var files = new DirectoryInfo(path).GetFiles("*.log")
            .OrderByDescending(file => file.LastWriteTimeUtc)
            .ToList();

        var deletions = files.Where(file => file.LastWriteTimeUtc < cutoff).ToList();
        foreach (var file in deletions)
        {
            TryDelete(file);
        }

        if (maxFiles == int.MaxValue)
        {
            return;
        }

        files = new DirectoryInfo(path).GetFiles("*.log")
            .OrderByDescending(file => file.LastWriteTimeUtc)
            .ToList();

        foreach (var file in files.Skip(maxFiles))
        {
            TryDelete(file);
        }
    }

    private static void TryDelete(FileInfo file)
    {
        try
        {
            file.Delete();
        }
        catch (IOException)
        {
            // Ignore cleanup failures.
        }
        catch (UnauthorizedAccessException)
        {
            // Ignore cleanup failures.
        }
    }
}
