using System;
using System.IO;
using Filebuloso.Models;

namespace Filebuloso.Services;

public sealed class CollisionHandler
{
    private readonly HashCalculator _hashCalculator;
    private readonly FileOperations _fileOperations;
    private readonly TimestampService _timestampService;
    private readonly Logger? _logger;

    public CollisionHandler(HashCalculator hashCalculator, FileOperations fileOperations, TimestampService timestampService, Logger? logger = null)
    {
        _hashCalculator = hashCalculator;
        _fileOperations = fileOperations;
        _timestampService = timestampService;
        _logger = logger;
    }

    public OperationResult HandleCollision(string sourcePath, string destinationPath)
    {
        var sourceHash = _hashCalculator.CalculateMd5Hash(sourcePath);
        var destHash = _hashCalculator.CalculateMd5Hash(destinationPath);

        if (string.Equals(sourceHash, destHash, StringComparison.OrdinalIgnoreCase))
        {
            _logger?.LogOperation("DUPLICATE", $"Duplicate removed: {sourcePath}");
            return _fileOperations.DeleteFile(sourcePath);
        }

        var sourceInfo = new FileInfo(sourcePath);
        var timestamp = sourceInfo.LastWriteTime == DateTime.MinValue ? sourceInfo.CreationTime : sourceInfo.LastWriteTime;
        var stamped = _timestampService.AddTimestampToFilename(destinationPath, timestamp);
        stamped = _timestampService.EnsureUniqueTimestampName(stamped);

        var moveResult = _fileOperations.MoveFile(sourcePath, stamped);
        if (moveResult.Success)
        {
            _logger?.LogOperation("VERSION", $"Version preserved: {sourcePath} -> {stamped}");
        }

        return moveResult;
    }
}
