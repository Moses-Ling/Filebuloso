using System.IO;
using System.Windows;
using System.Threading;
using System.Threading.Tasks;
using Filebuloso.Models;
using Filebuloso.Services;
using Filebuloso.Views;
using Forms = System.Windows.Forms;

namespace Filebuloso;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private string? _selectedDirectory;
    private readonly ConfigurationService _configurationService;
    private readonly Logger _logger;
    private AppConfig _config;
    private CancellationTokenSource? _cts;

    public MainWindow()
    {
        InitializeComponent();
        _logger = new Logger();
        _configurationService = new ConfigurationService(_logger);
        _config = _configurationService.LoadConfiguration();
        DirectoryTextBox.Text = _config.DefaultDirectory;
        Closed += MainWindow_Closed;
    }

    private void BrowseButton_Click(object sender, RoutedEventArgs e)
    {
        using var dialog = new Forms.FolderBrowserDialog
        {
            Description = "Select a folder to organize",
            UseDescriptionForTitle = true,
            ShowNewFolderButton = false
        };

        if (dialog.ShowDialog() == Forms.DialogResult.OK)
        {
            DirectoryTextBox.Text = dialog.SelectedPath;
        }
    }

    private void DirectoryTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        _selectedDirectory = DirectoryTextBox.Text.Trim();
        UpdateStartButtonState();
        SaveSelectedDirectory();
    }

    private void StartButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_selectedDirectory))
        {
            return;
        }

        var confirm = System.Windows.MessageBox.Show(
            $"Organize files in:\n{_selectedDirectory}\n\nThis may delete duplicate files.\nDry run mode: {_config.DryRunByDefault}",
            "Confirm Organization",
            MessageBoxButton.OKCancel,
            MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.OK)
        {
            return;
        }

        StartOrganizationAsync(_selectedDirectory);
    }

    private void ConfigureCategoriesButton_Click(object sender, RoutedEventArgs e)
    {
        var window = new CategoryEditorWindow
        {
            Owner = this
        };
        window.ShowDialog();
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        var window = new SettingsWindow
        {
            Owner = this
        };
        if (window.ShowDialog() == true)
        {
            _config = _configurationService.LoadConfiguration();
            DirectoryTextBox.Text = _config.DefaultDirectory;
        }
    }

    private void ExitButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        _cts?.Cancel();
    }

    private void UpdateStartButtonState()
    {
        StartButton.IsEnabled = !string.IsNullOrWhiteSpace(_selectedDirectory) &&
                                Directory.Exists(_selectedDirectory);
    }

    private async void StartOrganizationAsync(string directory)
    {
        ToggleUi(false);
        _cts = new CancellationTokenSource();

        var progress = new Progress<OrganizationProgress>(update =>
        {
            ProgressBar.Value = update.Percentage;
            StatusText.Text = update.CurrentOperation;
        });

        try
        {
            var orchestrator = BuildOrchestrator();
            var result = await Task.Run(() => orchestrator.OrganizeDirectory(
                directory,
                _config.DryRunByDefault,
                progress,
                _cts.Token));

            System.Windows.MessageBox.Show(
                result.SummaryText,
                "Organization Summary",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (OperationCanceledException)
        {
            StatusText.Text = "Canceled";
        }
        finally
        {
            _cts = null;
            ToggleUi(true);
        }
    }

    private OrganizationOrchestrator BuildOrchestrator()
    {
        var fileScanner = new FileScanner();
        var hashCalculator = new HashCalculator();
        var patternDetector = new PatternDetector();
        var timestampService = new TimestampService();
        var duplicateDetector = new DuplicateDetector(fileScanner, hashCalculator, patternDetector, timestampService);
        var fileOperations = new FileOperations(_logger);
        var collisionHandler = new CollisionHandler(hashCalculator, fileOperations, timestampService, _logger);
        var categorizer = new Categorizer(fileOperations, collisionHandler);

        return new OrganizationOrchestrator(fileScanner, duplicateDetector, categorizer, fileOperations, _logger);
    }

    private void ToggleUi(bool enabled)
    {
        BrowseButton.IsEnabled = enabled;
        StartButton.IsEnabled = enabled && !string.IsNullOrWhiteSpace(_selectedDirectory);
        CancelButton.IsEnabled = !enabled;
        StatusText.Text = enabled ? "Ready" : "Processing...";
    }

    private void SaveSelectedDirectory()
    {
        if (string.IsNullOrWhiteSpace(_selectedDirectory) || !Directory.Exists(_selectedDirectory))
        {
            return;
        }

        if (string.Equals(_config.DefaultDirectory, _selectedDirectory, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        _config.DefaultDirectory = _selectedDirectory;
        try
        {
            _configurationService.SaveConfiguration(_config);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to save default directory: {ex.Message}");
        }
    }

    private void MainWindow_Closed(object? sender, EventArgs e)
    {
        SaveSelectedDirectory();
    }
}
