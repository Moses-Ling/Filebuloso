using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Filebuloso.Models;

namespace Filebuloso.Services;

public sealed class OrganizationOrchestrator
{
    private readonly FileScanner _scanner;
    private readonly DuplicateDetector _duplicateDetector;
    private readonly Categorizer _categorizer;
    private readonly FileOperations _fileOperations;
    private readonly SubdirectoryDuplicateCleaner _subdirCleaner;
    private readonly Logger _logger;

    public OrganizationOrchestrator(
        FileScanner scanner,
        DuplicateDetector duplicateDetector,
        Categorizer categorizer,
        FileOperations fileOperations,
        Logger logger)
    {
        _scanner = scanner;
        _duplicateDetector = duplicateDetector;
        _categorizer = categorizer;
        _fileOperations = fileOperations;
        _subdirCleaner = new SubdirectoryDuplicateCleaner(new HashCalculator(), fileOperations, logger);
        _logger = logger;
    }

    public OrganizationResult OrganizeDirectory(
        string directory,
        bool dryRun,
        IProgress<OrganizationProgress>? progress,
        CancellationToken cancellationToken)
    {
        var result = new OrganizationResult();

        var totalFiles = _scanner.GetFileCount(directory);
        _logger.LogOperation("SCAN", $"Pre-scan counted {totalFiles} files in {directory}");
        progress?.Report(new OrganizationProgress
        {
            Percentage = 0,
            CurrentOperation = "Pre-scan complete",
            TotalFiles = totalFiles,
            FilesProcessed = 0,
            IsIndeterminate = false
        });

        cancellationToken.ThrowIfCancellationRequested();

        progress?.Report(new OrganizationProgress
        {
            Percentage = 25,
            CurrentOperation = "Detecting duplicates",
            TotalFiles = totalFiles,
            FilesProcessed = 0,
            IsIndeterminate = true
        });

        var duplicates = _duplicateDetector.DetectDuplicates(directory);
        result.TotalFilesScanned = duplicates.TotalFilesScanned;
        result.DuplicateGroupsFound = duplicates.DuplicateGroupsFound;
        _logger.LogOperation("DUPLICATE", $"Duplicate groups found: {duplicates.DuplicateGroupsFound}");

        if (!dryRun)
        {
            foreach (var file in duplicates.FilesToDelete)
            {
                var deleteResult = _fileOperations.DeleteFile(file);
                if (!deleteResult.Success)
                {
                    result.Errors.Add(deleteResult.Message);
                }
                else
                {
                    result.DuplicatesRemoved++;
                }
            }

            foreach (var rename in duplicates.FilesToRename)
            {
                try
                {
                    File.Move(rename.Key, rename.Value);
                    _logger.LogOperation("RENAME", $"{rename.Key} -> {rename.Value}");
                }
                catch (IOException ex)
                {
                    result.Errors.Add(ex.Message);
                    _logger.LogError($"Failed to rename {rename.Key}: {ex.Message}");
                }
            }
        }

        progress?.Report(new OrganizationProgress
        {
            Percentage = 50,
            CurrentOperation = "Categorizing files",
            TotalFiles = totalFiles,
            FilesProcessed = 0,
            IsIndeterminate = true
        });

        cancellationToken.ThrowIfCancellationRequested();

        var remainingFiles = _scanner.ScanDirectory(directory);
        if (!dryRun)
        {
            var config = new ConfigurationService(_logger).LoadConfiguration();
            var categorization = _categorizer.CategorizeFiles(remainingFiles, directory, config.Categories);
            result.VersionsPreserved = categorization.VersionsPreserved;
            result.FilesMoved = categorization.FilesMoved;
            result.UncategorizedFiles = categorization.UncategorizedFiles;
            result.Errors.AddRange(categorization.Errors);
            _logger.LogOperation("MOVE", $"Files moved: {categorization.FilesMoved}");
            _logger.LogOperation("VERSION", $"Versions preserved: {categorization.VersionsPreserved}");

            if (config.ScanSubdirectoriesForDuplicates)
            {
                progress?.Report(new OrganizationProgress
                {
                    Percentage = 90,
                    CurrentOperation = "Scanning subfolders for duplicates",
                    TotalFiles = totalFiles,
                    FilesProcessed = totalFiles,
                    IsIndeterminate = true
                });

                var subdirResult = _subdirCleaner.Clean(directory);
                if (!subdirResult.Result.Success)
                {
                    result.Errors.Add(subdirResult.Result.Message);
                }
                result.SubfolderDuplicateGroups = subdirResult.Groups;
                result.SubfolderDuplicatesDeleted = subdirResult.Deleted;
            }
        }
        else
        {
            var config = new ConfigurationService(_logger).LoadConfiguration();
            foreach (var file in remainingFiles)
            {
                var category = _categorizer.GetCategoryForFile(file.Name, config.Categories);
                if (string.IsNullOrEmpty(category))
                {
                    result.UncategorizedFiles++;
                }
            }
        }

        progress?.Report(new OrganizationProgress
        {
            Percentage = 100,
            CurrentOperation = "Complete",
            TotalFiles = totalFiles,
            FilesProcessed = totalFiles,
            IsIndeterminate = false
        });

        result.SummaryText = BuildSummary(result);
        return result;
    }

    private static string BuildSummary(OrganizationResult result)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Total files scanned: {result.TotalFilesScanned}");
        builder.AppendLine($"Duplicate groups: {result.DuplicateGroupsFound}");
        builder.AppendLine($"Duplicates removed: {result.DuplicatesRemoved}");
        builder.AppendLine($"Versions preserved: {result.VersionsPreserved}");
        builder.AppendLine($"Files moved: {result.FilesMoved}");
        builder.AppendLine($"Uncategorized files: {result.UncategorizedFiles}");
        if (result.SubfolderDuplicateGroups > 0 || result.SubfolderDuplicatesDeleted > 0)
        {
            builder.AppendLine($"Subfolder duplicate groups: {result.SubfolderDuplicateGroups}");
            builder.AppendLine($"Subfolder duplicates deleted: {result.SubfolderDuplicatesDeleted}");
        }
        if (result.Errors.Count > 0)
        {
            builder.AppendLine($"Errors: {result.Errors.Count}");
        }

        return builder.ToString();
    }
}
