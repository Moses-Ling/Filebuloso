using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using Filebuloso.Helpers;
using Filebuloso.Models;

namespace Filebuloso.Services;

public sealed class ConfigurationService
{
    private const string DefaultConfigResourceName = "Filebuloso.Config.DefaultConfig.json";
    private const int BackupCount = 3;

    private readonly Logger _logger;
    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public ConfigurationService(Logger logger)
    {
        _logger = logger;
    }

    public AppConfig LoadConfiguration()
    {
        AppPaths.EnsureDirectories();

        if (!File.Exists(AppPaths.ConfigPath))
        {
            var created = CreateDefaultConfiguration();
            SaveConfiguration(created);
            _logger.LogInfo("Default configuration created.");
            return created;
        }

        try
        {
            var json = File.ReadAllText(AppPaths.ConfigPath);
            var config = JsonSerializer.Deserialize<AppConfig>(json, _serializerOptions);
            if (!ValidateConfiguration(config))
            {
                return RecoverCorruptedConfiguration();
            }

            EnsureDefaultDirectory(config!);
            _logger.LogInfo("Configuration loaded.");
            return config!;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to load configuration: {ex.Message}");
            return RecoverCorruptedConfiguration();
        }
    }

    public void SaveConfiguration(AppConfig config)
    {
        if (!ValidateConfiguration(config))
        {
            throw new InvalidOperationException("Configuration is invalid.");
        }

        AppPaths.EnsureDirectories();
        RotateBackups();

        var json = JsonSerializer.Serialize(config, _serializerOptions);
        var tempPath = AppPaths.ConfigPath + ".tmp";
        File.WriteAllText(tempPath, json);
        File.Copy(tempPath, AppPaths.ConfigPath, overwrite: true);
        File.Delete(tempPath);

        _logger.LogInfo("Configuration saved.");
    }

    public AppConfig CreateDefaultConfiguration()
    {
        using var stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream(DefaultConfigResourceName);

        if (stream is null)
        {
            throw new InvalidOperationException($"Missing embedded resource: {DefaultConfigResourceName}");
        }

        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();
        var config = JsonSerializer.Deserialize<AppConfig>(json, _serializerOptions);
        if (!ValidateConfiguration(config))
        {
            throw new InvalidOperationException("Embedded configuration is invalid.");
        }

        EnsureDefaultDirectory(config!);
        return config!;
    }

    public bool ValidateConfiguration(AppConfig? config)
    {
        if (config is null)
        {
            return false;
        }

        if (config.Categories is null || config.Window is null || config.Logging is null)
        {
            return false;
        }

        return true;
    }

    private void RotateBackups()
    {
        if (!File.Exists(AppPaths.ConfigPath))
        {
            return;
        }

        for (var i = BackupCount; i >= 1; i--)
        {
            var source = GetBackupPath(i);
            var dest = GetBackupPath(i + 1);

            if (File.Exists(dest))
            {
                File.Delete(dest);
            }

            if (File.Exists(source))
            {
                File.Move(source, dest);
            }
        }

        File.Copy(AppPaths.ConfigPath, GetBackupPath(1), overwrite: true);
    }

    private string GetBackupPath(int index) =>
        Path.Combine(AppPaths.AppDataRoot, $"config.json.backup{index}");

    private AppConfig RecoverCorruptedConfiguration()
    {
        var corruptedPath = Path.Combine(AppPaths.AppDataRoot, "config.json.corrupted");
        if (File.Exists(AppPaths.ConfigPath))
        {
            File.Copy(AppPaths.ConfigPath, corruptedPath, overwrite: true);
        }

        var defaults = CreateDefaultConfiguration();
        SaveConfiguration(defaults);
        _logger.LogWarning("Configuration was invalid. Restored defaults.");
        return defaults;
    }

    private static void EnsureDefaultDirectory(AppConfig config)
    {
        if (string.IsNullOrWhiteSpace(config.DefaultDirectory) ||
            config.DefaultDirectory.Contains("Username", StringComparison.OrdinalIgnoreCase))
        {
            var downloads = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads");
            config.DefaultDirectory = downloads;
        }
    }
}
