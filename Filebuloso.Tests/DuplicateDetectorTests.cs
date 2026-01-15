using System;
using System.IO;
using Filebuloso.Services;
using Xunit;

namespace Filebuloso.Tests;

public sealed class DuplicateDetectorTests
{
    [Fact]
    public void DetectDuplicates_PrefersFileWithoutPattern()
    {
        using var tempDir = new TempDirectory();
        var fileA = Path.Combine(tempDir.Path, "report.pdf");
        var fileB = Path.Combine(tempDir.Path, "report(1).pdf");
        var fileC = Path.Combine(tempDir.Path, "report(2).pdf");
        File.WriteAllText(fileA, "Same");
        File.WriteAllText(fileB, "Same");
        File.WriteAllText(fileC, "Same");

        var detector = new DuplicateDetector(new FileScanner(), new HashCalculator(), new PatternDetector(), new TimestampService());
        var result = detector.DetectDuplicates(tempDir.Path);

        Assert.Contains(fileA, result.FilesToKeep);
        Assert.Contains(fileB, result.FilesToDelete);
        Assert.Contains(fileC, result.FilesToDelete);
        Assert.Equal(1, result.DuplicateGroupsFound);
    }

    [Fact]
    public void DetectDuplicates_ReturnsEmptyWhenNoDuplicates()
    {
        using var tempDir = new TempDirectory();
        var fileA = Path.Combine(tempDir.Path, "alpha.txt");
        var fileB = Path.Combine(tempDir.Path, "beta.txt");
        File.WriteAllText(fileA, "Alpha");
        File.WriteAllText(fileB, "Beta");

        var detector = new DuplicateDetector(new FileScanner(), new HashCalculator(), new PatternDetector(), new TimestampService());
        var result = detector.DetectDuplicates(tempDir.Path);

        Assert.Empty(result.FilesToDelete);
        Assert.Empty(result.FilesToKeep);
        Assert.Equal(0, result.DuplicateGroupsFound);
    }

    [Fact]
    public void DetectDuplicates_TracksRenameWhenKeptFileHasPattern()
    {
        using var tempDir = new TempDirectory();
        var fileA = Path.Combine(tempDir.Path, "report_1.pdf");
        var fileB = Path.Combine(tempDir.Path, "report(2).pdf");
        File.WriteAllText(fileA, "Same");
        File.WriteAllText(fileB, "Same");

        var detector = new DuplicateDetector(new FileScanner(), new HashCalculator(), new PatternDetector(), new TimestampService());
        var result = detector.DetectDuplicates(tempDir.Path);

        Assert.Single(result.FilesToRename);
        Assert.True(result.FilesToRename.ContainsKey(fileB));
        Assert.Equal(Path.Combine(tempDir.Path, "report.pdf"), result.FilesToRename[fileB]);
    }

    [Fact]
    public void DetectDuplicates_PicksHighestNumberWhenAllPatterns()
    {
        using var tempDir = new TempDirectory();
        var fileA = Path.Combine(tempDir.Path, "report(1).pdf");
        var fileB = Path.Combine(tempDir.Path, "report(3).pdf");
        File.WriteAllText(fileA, "Same");
        File.WriteAllText(fileB, "Same");

        var detector = new DuplicateDetector(new FileScanner(), new HashCalculator(), new PatternDetector(), new TimestampService());
        var result = detector.DetectDuplicates(tempDir.Path);

        Assert.Contains(fileB, result.FilesToKeep);
        Assert.Equal(Path.Combine(tempDir.Path, "report.pdf"), result.FilesToRename[fileB]);
    }

    [Fact]
    public void DetectDuplicates_AppendsTimestampWhenBaseExistsWithDifferentHash()
    {
        using var tempDir = new TempDirectory();
        var baseFile = Path.Combine(tempDir.Path, "report.pdf");
        var patterned = Path.Combine(tempDir.Path, "report(2).pdf");
        File.WriteAllText(baseFile, "Different");
        File.WriteAllText(patterned, "Same");

        var detector = new DuplicateDetector(new FileScanner(), new HashCalculator(), new PatternDetector(), new TimestampService());
        var result = detector.DetectDuplicates(tempDir.Path);

        Assert.True(result.FilesToRename.ContainsKey(patterned));
        var renamed = result.FilesToRename[patterned];
        Assert.StartsWith(Path.Combine(tempDir.Path, "report_"), renamed);
        Assert.EndsWith(".pdf", renamed);
    }

    [Fact]
    public void DetectDuplicates_TimestampsPatternFilesWithDifferentHashes()
    {
        using var tempDir = new TempDirectory();
        var baseFile = Path.Combine(tempDir.Path, "report.pdf");
        var patternFile = Path.Combine(tempDir.Path, "report(1).pdf");
        File.WriteAllText(baseFile, "Base");
        File.WriteAllText(patternFile, "Different");

        var detector = new DuplicateDetector(new FileScanner(), new HashCalculator(), new PatternDetector(), new TimestampService());
        var result = detector.DetectDuplicates(tempDir.Path);

        Assert.True(result.FilesToRename.ContainsKey(patternFile));
        var renamed = result.FilesToRename[patternFile];
        Assert.StartsWith(Path.Combine(tempDir.Path, "report_"), renamed);
        Assert.EndsWith(".pdf", renamed);
        Assert.DoesNotContain("(1)", renamed);
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
