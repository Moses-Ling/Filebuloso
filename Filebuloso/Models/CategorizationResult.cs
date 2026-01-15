using System.Collections.Generic;

namespace Filebuloso.Models;

public sealed class CategorizationResult
{
    public int FilesProcessed { get; set; }
    public int VersionsPreserved { get; set; }
    public int DuplicatesRemoved { get; set; }
    public int UncategorizedFiles { get; set; }
    public int FilesMoved { get; set; }
    public List<string> Errors { get; } = new();
}
