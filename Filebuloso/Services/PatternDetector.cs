using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Filebuloso.Models;

namespace Filebuloso.Services;

public sealed class PatternDetector
{
    private static readonly Regex PatternRegex = new(@"^(?<base>.+?)[\(_-](?<num>\d+)\)?$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public PatternInfo DetectPattern(string filename)
    {
        var name = Path.GetFileNameWithoutExtension(filename);
        if (string.IsNullOrWhiteSpace(name))
        {
            return new PatternInfo { HasPattern = false, BaseName = name };
        }

        var match = PatternRegex.Match(name);
        if (!match.Success)
        {
            return new PatternInfo { HasPattern = false, BaseName = name };
        }

        if (!int.TryParse(match.Groups["num"].Value, NumberStyles.None, CultureInfo.InvariantCulture, out var number))
        {
            return new PatternInfo { HasPattern = false, BaseName = name };
        }

        return new PatternInfo
        {
            HasPattern = true,
            Number = number,
            BaseName = match.Groups["base"].Value
        };
    }

    public string SelectFileToKeep(string[] files)
    {
        if (files.Length == 0)
        {
            return string.Empty;
        }

        var patterns = files.Select(file =>
        {
            var info = DetectPattern(file);
            return (file, info);
        }).ToList();

        var anyPattern = patterns.Any(item => item.info.HasPattern);
        if (!anyPattern)
        {
            return patterns
                .OrderBy(item => Path.GetFileName(item.file), StringComparer.OrdinalIgnoreCase)
                .First()
                .file;
        }

        var noPattern = patterns.Where(item => !item.info.HasPattern).ToList();
        if (noPattern.Count > 0)
        {
            return noPattern
                .OrderBy(item => Path.GetFileName(item.file), StringComparer.OrdinalIgnoreCase)
                .First()
                .file;
        }

        var allHaveNumbers = patterns.All(item => item.info.Number.HasValue);
        if (allHaveNumbers)
        {
            return patterns
                .OrderByDescending(item => item.info.Number)
                .ThenBy(item => item.file, StringComparer.OrdinalIgnoreCase)
                .First()
                .file;
        }

        return patterns
            .OrderBy(item => Path.GetFileName(item.file), StringComparer.OrdinalIgnoreCase)
            .First()
            .file;
    }

    public string RemovePattern(string filename)
    {
        var directory = Path.GetDirectoryName(filename) ?? string.Empty;
        var extension = Path.GetExtension(filename);
        var name = Path.GetFileNameWithoutExtension(filename);
        var info = DetectPattern(name);
        if (!info.HasPattern)
        {
            return filename;
        }

        var cleaned = info.BaseName + extension;
        return string.IsNullOrEmpty(directory) ? cleaned : Path.Combine(directory, cleaned);
    }
}
