using Filebuloso.Services;
using Xunit;

namespace Filebuloso.Tests;

public sealed class PatternDetectorTests
{
    private readonly PatternDetector _detector = new();

    [Theory]
    [InlineData("report(1).pdf", true, 1, "report")]
    [InlineData("report(10).pdf", true, 10, "report")]
    [InlineData("report_2.pdf", true, 2, "report")]
    [InlineData("report-3.pdf", true, 3, "report")]
    [InlineData("report.pdf", false, null, "report")]
    public void DetectPattern_ReturnsExpectedValues(string name, bool hasPattern, int? number, string baseName)
    {
        var info = _detector.DetectPattern(name);

        Assert.Equal(hasPattern, info.HasPattern);
        Assert.Equal(number, info.Number);
        Assert.Equal(baseName, info.BaseName);
    }

    [Fact]
    public void SelectFileToKeep_PrefersNoPattern()
    {
        var files = new[] { "report(1).pdf", "report.pdf", "report(2).pdf" };

        var keep = _detector.SelectFileToKeep(files);

        Assert.Equal("report.pdf", keep);
    }

    [Fact]
    public void SelectFileToKeep_PrefersHighestNumber()
    {
        var files = new[] { "report(2).pdf", "report_1.pdf", "report(5).pdf" };

        var keep = _detector.SelectFileToKeep(files);

        Assert.Equal("report(5).pdf", keep);
    }

    [Fact]
    public void SelectFileToKeep_UsesAlphabeticalWhenNoPattern()
    {
        var files = new[] { "report-final.pdf", "report-copy.pdf", "report-backup.pdf" };

        var keep = _detector.SelectFileToKeep(files);

        Assert.Equal("report-backup.pdf", keep);
    }
}
