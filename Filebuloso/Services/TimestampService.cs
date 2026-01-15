using System;
using System.IO;

namespace Filebuloso.Services;

public sealed class TimestampService
{
    public string AddTimestampToFilename(string filePath, DateTime timestamp)
    {
        var directory = Path.GetDirectoryName(filePath) ?? string.Empty;
        var name = Path.GetFileNameWithoutExtension(filePath);
        var extension = Path.GetExtension(filePath);
        var stamp = timestamp.ToString("yyyyMMdd_HHmmss");
        var stamped = $"{name}_{stamp}{extension}";
        return string.IsNullOrEmpty(directory) ? stamped : Path.Combine(directory, stamped);
    }

    public string EnsureUniqueTimestampName(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return filePath;
        }

        var directory = Path.GetDirectoryName(filePath) ?? string.Empty;
        var name = Path.GetFileNameWithoutExtension(filePath);
        var extension = Path.GetExtension(filePath);
        var counter = 1;

        string candidate;
        do
        {
            candidate = $"{name}_{counter}{extension}";
            candidate = string.IsNullOrEmpty(directory) ? candidate : Path.Combine(directory, candidate);
            counter++;
        } while (File.Exists(candidate));

        return candidate;
    }
}
