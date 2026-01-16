namespace Filebuloso.Models;

public sealed class OrganizationProgress
{
    public int Percentage { get; init; }
    public string CurrentOperation { get; init; } = string.Empty;
    public int FilesProcessed { get; init; }
    public int TotalFiles { get; init; }
    public bool IsIndeterminate { get; init; }
}
