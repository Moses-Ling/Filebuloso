using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Filebuloso.Models;

namespace Filebuloso.Services;

public sealed class DuplicateDetector
{
    private readonly FileScanner _scanner;
    private readonly HashCalculator _hashCalculator;
    private readonly PatternDetector _patternDetector;
    private readonly TimestampService _timestampService;

    public DuplicateDetector(
        FileScanner scanner,
        HashCalculator hashCalculator,
        PatternDetector patternDetector,
        TimestampService timestampService)
    {
        _scanner = scanner;
        _hashCalculator = hashCalculator;
        _patternDetector = patternDetector;
        _timestampService = timestampService;
    }

    public DuplicateDetectionResult DetectDuplicates(string directory)
    {
        var result = new DuplicateDetectionResult();
        var files = _scanner.ScanDirectory(directory);
        result.TotalFilesScanned = files.Count;

        var hashes = _hashCalculator.BatchCalculateHashes(files);
        var groups = hashes.GroupBy(entry => entry.Value)
            .Where(group => group.Count() > 1)
            .ToList();

        result.DuplicateGroupsFound = groups.Count;

        foreach (var group in groups)
        {
            var filePaths = group.Select(entry => entry.Key).ToArray();
            var keep = _patternDetector.SelectFileToKeep(filePaths);
            if (string.IsNullOrWhiteSpace(keep))
            {
                continue;
            }

            result.FilesToKeep.Add(keep);
            var cleaned = _patternDetector.RemovePattern(keep);
            if (!string.Equals(cleaned, keep, StringComparison.OrdinalIgnoreCase))
            {
                var renameTarget = cleaned;
                if (hashes.TryGetValue(cleaned, out var baseHash) &&
                    hashes.TryGetValue(keep, out var keepHash) &&
                    !string.Equals(baseHash, keepHash, StringComparison.OrdinalIgnoreCase))
                {
                    var keepInfo = new FileInfo(keep);
                    var timestamp = keepInfo.LastWriteTime == DateTime.MinValue
                        ? keepInfo.CreationTime
                        : keepInfo.LastWriteTime;
                    renameTarget = _timestampService.AddTimestampToFilename(cleaned, timestamp);
                    renameTarget = _timestampService.EnsureUniqueTimestampName(renameTarget);
                }

                result.FilesToRename[keep] = renameTarget;
            }
            foreach (var file in filePaths)
            {
                if (!string.Equals(file, keep, StringComparison.OrdinalIgnoreCase))
                {
                    result.FilesToDelete.Add(file);
                }
            }
        }

        AddTimestampRenamesForNonDuplicatePatterns(files, hashes, result);

        return result;
    }

    private void AddTimestampRenamesForNonDuplicatePatterns(
        IReadOnlyCollection<FileInfo> files,
        IReadOnlyDictionary<string, string> hashes,
        DuplicateDetectionResult result)
    {
        var remaining = files
            .Where(file => !result.FilesToDelete.Contains(file.FullName, StringComparer.OrdinalIgnoreCase))
            .ToList();

        var groups = remaining
            .Select(file => (file, info: _patternDetector.DetectPattern(file.Name)))
            .Where(item => item.info.HasPattern)
            .GroupBy(item => item.info.BaseName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var group in groups)
        {
            var baseName = group.Key;
            var hasBaseFile = remaining.Any(file =>
            {
                var info = _patternDetector.DetectPattern(file.Name);
                return !info.HasPattern && string.Equals(info.BaseName, baseName, StringComparison.OrdinalIgnoreCase);
            });

            if (!hasBaseFile)
            {
                continue;
            }

            foreach (var item in group)
            {
                var path = item.file.FullName;
                if (result.FilesToRename.ContainsKey(path))
                {
                    continue;
                }

                var timestamp = item.file.LastWriteTime == DateTime.MinValue
                    ? item.file.CreationTime
                    : item.file.LastWriteTime;
                var cleaned = _patternDetector.RemovePattern(path);
                var stamped = _timestampService.AddTimestampToFilename(cleaned, timestamp);
                stamped = _timestampService.EnsureUniqueTimestampName(stamped);
                result.FilesToRename[path] = stamped;
            }
        }
    }
}
