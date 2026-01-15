using System;
using System.IO;
using Filebuloso.Services;
using Xunit;

namespace Filebuloso.Tests;

public sealed class HashCalculatorTests
{
    [Fact]
    public void CalculateMd5Hash_ReturnsExpectedValue()
    {
        using var tempDir = new TempDirectory();
        var filePath = Path.Combine(tempDir.Path, "hello.txt");
        File.WriteAllText(filePath, "Hello World");

        var calculator = new HashCalculator();
        var hash = calculator.CalculateMd5Hash(filePath);

        Assert.Equal("b10a8db164e0754105b7a99be72e3fe5", hash);
    }

    [Fact]
    public void BatchCalculateHashes_ReturnsHashesForFiles()
    {
        using var tempDir = new TempDirectory();
        var fileA = Path.Combine(tempDir.Path, "a.txt");
        var fileB = Path.Combine(tempDir.Path, "b.txt");
        File.WriteAllText(fileA, "A");
        File.WriteAllText(fileB, "B");

        var files = new[] { new FileInfo(fileA), new FileInfo(fileB) };
        var calculator = new HashCalculator();

        var results = calculator.BatchCalculateHashes(files);

        Assert.Equal(2, results.Count);
        Assert.True(results.ContainsKey(fileA));
        Assert.True(results.ContainsKey(fileB));
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
