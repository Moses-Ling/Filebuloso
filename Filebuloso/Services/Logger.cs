using System;
using System.IO;
using Filebuloso.Helpers;

namespace Filebuloso.Services;

public sealed class Logger
{
    private readonly object _sync = new();
    private readonly string _logPath;
    private readonly string _errorLogPath;

    public Logger()
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        _logPath = Path.Combine(AppPaths.LogsRoot, $"Filebuloso_{timestamp}.log");
        _errorLogPath = Path.Combine(AppPaths.ErrorLogsRoot, $"Filebuloso_Errors_{DateTime.Now:yyyyMMdd}.log");
    }

    public void LogInfo(string message) => Write("INFO", message, isError: false);

    public void LogWarning(string message) => Write("WARNING", message, isError: false);

    public void LogError(string message) => Write("ERROR", message, isError: true);

    public void LogOperation(string category, string message) =>
        Write(category.ToUpperInvariant(), message, isError: false);

    private void Write(string level, string message, bool isError)
    {
        var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
        lock (_sync)
        {
            File.AppendAllLines(_logPath, new[] { line });
            if (isError)
            {
                File.AppendAllLines(_errorLogPath, new[] { line });
            }
        }
    }
}
