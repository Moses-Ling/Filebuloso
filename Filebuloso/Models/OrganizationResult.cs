using System.Collections.Generic;

namespace Filebuloso.Models;

public sealed class OrganizationResult
{
    public int TotalFilesScanned { get; set; }
    public int DuplicateGroupsFound { get; set; }
    public int DuplicatesRemoved { get; set; }
    public int VersionsPreserved { get; set; }
    public int FilesMoved { get; set; }
    public int UncategorizedFiles { get; set; }
    public List<string> Errors { get; } = new();
    public string SummaryText { get; set; } = string.Empty;
}
