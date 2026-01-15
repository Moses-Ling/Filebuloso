using System.Collections.Generic;

namespace Filebuloso.Models;

public sealed class DuplicateDetectionResult
{
    public List<string> FilesToKeep { get; } = new();
    public List<string> FilesToDelete { get; } = new();
    public Dictionary<string, string> FilesToRename { get; } = new();
    public int TotalFilesScanned { get; set; }
    public int DuplicateGroupsFound { get; set; }
}
