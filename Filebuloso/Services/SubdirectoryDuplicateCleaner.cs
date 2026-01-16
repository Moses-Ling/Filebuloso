using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Filebuloso.Models;

namespace Filebuloso.Services;

public sealed class SubdirectoryDuplicateCleaner
{
    private readonly HashCalculator _hashCalculator;
    private readonly FileOperations _fileOperations;
    private readonly Logger _logger;

    public SubdirectoryDuplicateCleaner(HashCalculator hashCalculator, FileOperations fileOperations, Logger logger)
    {
        _hashCalculator = hashCalculator;
        _fileOperations = fileOperations;
        _logger = logger;
    }

    public (OperationResult Result, int Groups, int Deleted) Clean(string rootDirectory)
    {
        if (!Directory.Exists(rootDirectory))
        {
            return (OperationResult.Fail("Root directory not found."), 0, 0);
        }

        var files = Directory.EnumerateFiles(rootDirectory, "*", SearchOption.AllDirectories)
            .Where(path => !string.Equals(Path.GetDirectoryName(path), rootDirectory, StringComparison.OrdinalIgnoreCase))
            .Select(path => new FileInfo(path))
            .ToList();

        var hashes = _hashCalculator.BatchCalculateHashes(files);
        var groups = hashes.GroupBy(entry => entry.Value)
            .Where(group => group.Count() > 1)
            .ToList();

        var deletedCount = 0;
        foreach (var group in groups)
        {
            var filesByDate = group
                .Select(entry => new FileInfo(entry.Key))
                .OrderBy(info => info.LastWriteTimeUtc == DateTime.MinValue ? info.CreationTimeUtc : info.LastWriteTimeUtc)
                .ToList();

            var keep = filesByDate.First();
            foreach (var duplicate in filesByDate.Skip(1))
            {
                var deleteResult = _fileOperations.DeleteFile(duplicate.FullName);
                if (!deleteResult.Success)
                {
                    _logger.LogError($"Failed to delete duplicate {duplicate.FullName}: {deleteResult.Message}");
                }
                else
                {
                    deletedCount++;
                }
            }

            _logger.LogOperation("DUPLICATE", $"Subfolder duplicates cleaned; kept oldest: {keep.FullName}");
        }

        return (OperationResult.Ok("Subfolder duplicates processed."), groups.Count, deletedCount);
    }
}
