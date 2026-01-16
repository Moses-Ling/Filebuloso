using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Filebuloso.Models;
using Filebuloso.Services;

namespace Filebuloso.Views;

/// <summary>
/// Interaction logic for SettingsWindow.xaml
/// </summary>
public partial class SettingsWindow : Window
{
    private readonly ConfigurationService _configurationService;
    private readonly Logger _logger;
    private AppConfig _config;

    public SettingsWindow()
    {
        InitializeComponent();

        _logger = new Logger();
        _configurationService = new ConfigurationService(_logger);
        _config = _configurationService.LoadConfiguration();
        LoadValues();
    }

    private void LoadValues()
    {
        ConfirmCheckBox.IsChecked = _config.ConfirmBeforeProcessing;
        DryRunCheckBox.IsChecked = _config.DryRunByDefault;
        SubdirDuplicatesCheckBox.IsChecked = _config.ScanSubdirectoriesForDuplicates;
        SummaryPopupCheckBox.IsChecked = _config.ShowSummaryPopup;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        _config.ConfirmBeforeProcessing = ConfirmCheckBox.IsChecked == true;
        _config.DryRunByDefault = DryRunCheckBox.IsChecked == true;
        _config.ScanSubdirectoriesForDuplicates = SubdirDuplicatesCheckBox.IsChecked == true;
        _config.ShowSummaryPopup = SummaryPopupCheckBox.IsChecked == true;

        try
        {
            _configurationService.SaveConfiguration(_config);
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                $"Failed to save settings: {ex.Message}",
                "Settings Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void ResetButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _config = _configurationService.CreateDefaultConfiguration();
            _configurationService.SaveConfiguration(_config);
            LoadValues();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                $"Failed to reset settings: {ex.Message}",
                "Settings Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void ViewLicenseButton_Click(object sender, RoutedEventArgs e)
    {
        var licenseText = LoadLicenseText();
        var window = new Window
        {
            Title = "MIT License",
            Owner = this,
            Width = 640,
            Height = 520,
            MinWidth = 480,
            MinHeight = 360,
            Content = new System.Windows.Controls.TextBox
            {
                Text = licenseText,
                IsReadOnly = true,
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                Margin = new Thickness(12)
            }
        };

        window.ShowDialog();
    }

    private static string LoadLicenseText()
    {
        var baseDir = AppContext.BaseDirectory;
        var current = new DirectoryInfo(baseDir);
        while (current is not null)
        {
            var candidate = Path.Combine(current.FullName, "license.txt");
            if (File.Exists(candidate))
            {
                return File.ReadAllText(candidate);
            }

            current = current.Parent;
        }

        return "license.txt not found.";
    }
}
