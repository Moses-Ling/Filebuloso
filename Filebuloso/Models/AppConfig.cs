using System.Collections.Generic;

namespace Filebuloso.Models;

public sealed class AppConfig
{
    public string Version { get; set; } = "1.0";
    public string DefaultDirectory { get; set; } = string.Empty;
    public bool ConfirmBeforeProcessing { get; set; } = true;
    public bool DryRunByDefault { get; set; } = false;
    public List<FileCategory> Categories { get; set; } = new();
    public WindowSettings Window { get; set; } = new();
    public LoggingSettings Logging { get; set; } = new();
}
