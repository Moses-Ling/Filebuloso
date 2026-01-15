using System;
using System.Collections.Generic;
using System.IO;
using Filebuloso.Models;
using Filebuloso.Services;
using Xunit;

namespace Filebuloso.Tests;

public sealed class CategorizerTests
{
    [Fact]
    public void CategorizeFiles_MovesKnownExtensions()
    {
        using var tempDir = new TempDirectory();
        var file = Path.Combine(tempDir.Path, "report.pdf");
        File.WriteAllText(file, "data");

        var categories = new List<FileCategory>
        {
            new() { Name = "pdf", Extensions = new List<string> { "pdf" } }
        };

        var categorizer = new Categorizer(new FileOperations(), new CollisionHandler(new HashCalculator(), new FileOperations(), new TimestampService()));
        var result = categorizer.CategorizeFiles(new[] { new FileInfo(file) }, tempDir.Path, categories);

        Assert.Equal(1, result.FilesProcessed);
        Assert.True(File.Exists(Path.Combine(tempDir.Path, "pdf", "report.pdf")));
    }

    [Fact]
    public void GetCategoryForFile_UnclassifiedForNoExtension()
    {
        var categories = new List<FileCategory>
        {
            new() { Name = "pdf", Extensions = new List<string> { "pdf" } }
        };

        var categorizer = new Categorizer(new FileOperations(), new CollisionHandler(new HashCalculator(), new FileOperations(), new TimestampService()));
        var category = categorizer.GetCategoryForFile("README", categories);

        Assert.Equal("unclassified", category);
    }

    private sealed class TempDirectory : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        public TempDirectory()
        {
            Directory.CreateDirectory(Path);
        }

        public void Dispose()
        {
            try
            {
                Directory.Delete(Path, recursive: true);
            }
            catch (IOException)
            {
                // Ignore cleanup failures in tests.
            }
        }
    }
}
