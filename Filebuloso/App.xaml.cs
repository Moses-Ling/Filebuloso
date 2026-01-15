using System;
using System.Windows;
using Filebuloso.Helpers;
using Filebuloso.Services;

namespace Filebuloso;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    private Logger? _logger;
    private ConfigurationService? _configurationService;

    protected override void OnStartup(StartupEventArgs e)
    {
        try
        {
            AppPaths.EnsureDirectories();
            _logger = new Logger();
            _configurationService = new ConfigurationService(_logger);
            var config = _configurationService.LoadConfiguration();

            var maintenance = new LogMaintenanceService();
            _ = System.Threading.Tasks.Task.Run(() => maintenance.CleanupLogs(config.Logging));
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                $"Filebuloso failed to initialize: {ex.Message}",
                "Startup Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown();
            return;
        }

        base.OnStartup(e);
    }
}
