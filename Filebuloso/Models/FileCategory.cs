using System.Collections.Generic;

namespace Filebuloso.Models;

public sealed class FileCategory
{
    public string Name { get; set; } = string.Empty;
    public List<string> Extensions { get; set; } = new();
}
