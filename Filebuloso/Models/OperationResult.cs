namespace Filebuloso.Models;

public sealed class OperationResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? DestinationPath { get; init; }

    public static OperationResult Ok(string message, string? destination = null) =>
        new() { Success = true, Message = message, DestinationPath = destination };

    public static OperationResult Fail(string message) =>
        new() { Success = false, Message = message };
}
