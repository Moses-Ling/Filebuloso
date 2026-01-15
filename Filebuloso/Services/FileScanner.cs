using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Filebuloso.Services;

public sealed class FileScanner
{
    public List<FileInfo> ScanDirectory(string path)
    {
        var results = new List<FileInfo>();
        if (!Directory.Exists(path))
        {
            return results;
        }

        foreach (var filePath in Directory.EnumerateFiles(path))
        {
            try
            {
                var info = new FileInfo(filePath);
                results.Add(info);
            }
            catch (IOException)
            {
                // Skip inaccessible files.
            }
            catch (UnauthorizedAccessException)
            {
                // Skip inaccessible files.
            }
        }

        return results;
    }

    public int GetFileCount(string path)
    {
        if (!Directory.Exists(path))
        {
            return 0;
        }

        try
        {
            return Directory.EnumerateFiles(path).Count();
        }
        catch (IOException)
        {
            return 0;
        }
        catch (UnauthorizedAccessException)
        {
            return 0;
        }
    }
}
