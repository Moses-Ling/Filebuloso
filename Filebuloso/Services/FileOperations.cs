using System.IO;
using Filebuloso.Models;

namespace Filebuloso.Services;

public sealed class FileOperations
{
    private readonly Logger? _logger;

    public FileOperations(Logger? logger = null)
    {
        _logger = logger;
    }

    public OperationResult MoveFile(string source, string destination)
    {
        try
        {
            var destDir = Path.GetDirectoryName(destination);
            if (!string.IsNullOrEmpty(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            File.Move(source, destination);
            _logger?.LogOperation("MOVE", $"{source} -> {destination}");
            return OperationResult.Ok("Moved file.", destination);
        }
        catch (IOException ex)
        {
            _logger?.LogError($"Failed to move {source}: {ex.Message}");
            return OperationResult.Fail(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger?.LogError($"Failed to move {source}: {ex.Message}");
            return OperationResult.Fail(ex.Message);
        }
    }

    public OperationResult DeleteFile(string path)
    {
        try
        {
            File.Delete(path);
            _logger?.LogOperation("DELETE", $"Removed {path}");
            return OperationResult.Ok("Deleted file.");
        }
        catch (IOException ex)
        {
            _logger?.LogError($"Failed to delete {path}: {ex.Message}");
            return OperationResult.Fail(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger?.LogError($"Failed to delete {path}: {ex.Message}");
            return OperationResult.Fail(ex.Message);
        }
    }

    public bool FileExists(string path) => File.Exists(path);
}
