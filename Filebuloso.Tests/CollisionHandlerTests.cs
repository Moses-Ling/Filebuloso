using System;
using System.IO;
using Filebuloso.Services;
using Xunit;

namespace Filebuloso.Tests;

public sealed class CollisionHandlerTests
{
    [Fact]
    public void HandleCollision_DeletesDuplicateWhenHashesMatch()
    {
        using var tempDir = new TempDirectory();
        var source = Path.Combine(tempDir.Path, "report.pdf");
        var dest = Path.Combine(tempDir.Path, "pdf", "report.pdf");
        Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
        File.WriteAllText(source, "Same");
        File.WriteAllText(dest, "Same");

        var handler = new CollisionHandler(new HashCalculator(), new FileOperations(), new TimestampService());
        var result = handler.HandleCollision(source, dest);

        Assert.True(result.Success);
        Assert.False(File.Exists(source));
    }

    [Fact]
    public void HandleCollision_PreservesVersionWhenHashesDiffer()
    {
        using var tempDir = new TempDirectory();
        var source = Path.Combine(tempDir.Path, "budget.xlsx");
        var dest = Path.Combine(tempDir.Path, "spreadsheets", "budget.xlsx");
        Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
        File.WriteAllText(source, "New");
        File.WriteAllText(dest, "Old");

        var handler = new CollisionHandler(new HashCalculator(), new FileOperations(), new TimestampService());
        var result = handler.HandleCollision(source, dest);

        Assert.True(result.Success);
        Assert.False(File.Exists(source));
        Assert.NotNull(result.DestinationPath);
        Assert.True(File.Exists(result.DestinationPath!));
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
