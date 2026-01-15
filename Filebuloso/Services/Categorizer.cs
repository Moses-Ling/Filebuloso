using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Filebuloso.Models;

namespace Filebuloso.Services;

public sealed class Categorizer
{
    private readonly FileOperations _fileOperations;
    private readonly CollisionHandler _collisionHandler;

    public Categorizer(FileOperations fileOperations, CollisionHandler collisionHandler)
    {
        _fileOperations = fileOperations;
        _collisionHandler = collisionHandler;
    }

    public string? GetCategoryForFile(string filename, IReadOnlyCollection<FileCategory> categories)
    {
        var extension = Path.GetExtension(filename).TrimStart('.').ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(extension))
        {
            return "unclassified";
        }

        var match = categories.FirstOrDefault(category =>
            category.Extensions.Any(ext => string.Equals(ext, extension, StringComparison.OrdinalIgnoreCase)));

        return match?.Name;
    }

    public CategorizationResult CategorizeFiles(IEnumerable<FileInfo> files, string targetDir, IReadOnlyCollection<FileCategory> categories)
    {
        var result = new CategorizationResult();

        foreach (var file in files)
        {
            result.FilesProcessed++;
            var category = GetCategoryForFile(file.Name, categories);
            if (string.IsNullOrEmpty(category))
            {
                result.UncategorizedFiles++;
                continue;
            }

            var destDir = Path.Combine(targetDir, category);
            var destPath = Path.Combine(destDir, file.Name);

            if (_fileOperations.FileExists(destPath))
            {
                var collisionResult = _collisionHandler.HandleCollision(file.FullName, destPath);
                if (!collisionResult.Success)
                {
                    result.Errors.Add(collisionResult.Message);
                    continue;
                }

                if (collisionResult.DestinationPath is null)
                {
                    result.DuplicatesRemoved++;
                }
                else if (!string.Equals(destPath, collisionResult.DestinationPath, StringComparison.OrdinalIgnoreCase))
                {
                    result.VersionsPreserved++;
                }
                else
                {
                    result.FilesMoved++;
                }
            }
            else
            {
                var moveResult = _fileOperations.MoveFile(file.FullName, destPath);
                if (!moveResult.Success)
                {
                    result.Errors.Add(moveResult.Message);
                }
                else
                {
                    result.FilesMoved++;
                }
            }
        }

        return result;
    }
}
