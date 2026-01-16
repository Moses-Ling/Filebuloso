using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using Filebuloso.Helpers;
using Filebuloso.Models;
using Filebuloso.Services;
using Filebuloso.Views;

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
        var picker = new FolderPicker();
        var selected = picker.PickFolder(this);
        if (!string.IsNullOrWhiteSpace(selected))
        {
            DirectoryTextBox.Text = selected;
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

        if (_config.ConfirmBeforeProcessing)
        {
            var confirm = System.Windows.MessageBox.Show(
                $"Organize files in:\n{_selectedDirectory}\n\nThis may delete duplicate files.\nDry run mode: {_config.DryRunByDefault}",
                "Confirm Organization",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.OK)
            {
                return;
            }
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
            ProgressBar.IsIndeterminate = update.IsIndeterminate;
            if (!update.IsIndeterminate)
            {
                ProgressBar.Value = update.Percentage;
            }
            ProgressBarText.Text = $"{update.CurrentOperation} Please wait!";
        });

        try
        {
            var orchestrator = BuildOrchestrator();
            var result = await Task.Run(() => orchestrator.OrganizeDirectory(
                directory,
                _config.DryRunByDefault,
                progress,
                _cts.Token));

            DisplaySummary(result.SummaryText);
            if (_config.ShowSummaryPopup)
            {
                System.Windows.MessageBox.Show(
                    result.SummaryText,
                    "Organization Summary",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }
        catch (OperationCanceledException)
        {
            ProgressBarText.Text = "Canceled.";
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
        ProgressBarText.Text = enabled ? "Ready" : "Processing... Please wait!";
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

    private void DisplaySummary(string summary)
    {
        var document = new FlowDocument();
        foreach (var line in summary.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None))
        {
            document.Blocks.Add(new Paragraph(new Run(line)));
        }

        LogTextBox.Document = document;
    }
}
