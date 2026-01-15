namespace Filebuloso.Models;

public sealed class LoggingSettings
{
    public int KeepDays { get; set; } = 30;
    public int MaxLogFiles { get; set; } = 50;
    public int ErrorLogDays { get; set; } = 90;
}
