using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Filebuloso.Services;

public sealed class HashCalculator
{
    public string CalculateMd5Hash(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        using var md5 = MD5.Create();
        var hashBytes = md5.ComputeHash(stream);
        var builder = new StringBuilder(hashBytes.Length * 2);
        foreach (var value in hashBytes)
        {
            builder.Append(value.ToString("x2"));
        }

        return builder.ToString();
    }

    public Dictionary<string, string> BatchCalculateHashes(IEnumerable<FileInfo> files)
    {
        var results = new Dictionary<string, string>();
        foreach (var file in files)
        {
            try
            {
                results[file.FullName] = CalculateMd5Hash(file.FullName);
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
}
