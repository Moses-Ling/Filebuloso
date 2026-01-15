namespace Filebuloso.Models;

public sealed class PatternInfo
{
    public bool HasPattern { get; init; }
    public int? Number { get; init; }
    public string BaseName { get; init; } = string.Empty;
}
