using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using Filebuloso.Models;
using Filebuloso.Services;
using Filebuloso.ViewModels;

namespace Filebuloso.Views;

/// <summary>
/// Interaction logic for CategoryEditorWindow.xaml
/// </summary>
public partial class CategoryEditorWindow : Window
{
    private static readonly Regex ExtensionPattern = new("^[A-Za-z0-9_]+$");

    private readonly ConfigurationService _configurationService;
    private readonly Logger _logger;
    private AppConfig _config;

    public ObservableCollection<CategoryRow> Categories { get; } = new();

    public CategoryEditorWindow()
    {
        InitializeComponent();

        _logger = new Logger();
        _configurationService = new ConfigurationService(_logger);
        _config = _configurationService.LoadConfiguration();

        LoadCategories();
        CategoriesGrid.ItemsSource = Categories;
    }

    private void LoadCategories()
    {
        Categories.Clear();
        foreach (var category in _config.Categories)
        {
            Categories.Add(new CategoryRow
            {
                Name = category.Name,
                Extensions = string.Join(", ", category.Extensions)
            });
        }
    }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new AddEditCategoryDialog("Add Category")
        {
            Owner = this
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        if (Categories.Any(c => c.Name.Equals(dialog.CategoryName, StringComparison.OrdinalIgnoreCase)))
        {
            System.Windows.MessageBox.Show(
                "Category names must be unique.",
                "Validation Error",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        var extensions = ParseExtensions(dialog.ExtensionsText);
        if (extensions is null)
        {
            return;
        }

        Categories.Add(new CategoryRow
        {
            Name = dialog.CategoryName,
            Extensions = string.Join(", ", extensions)
        });

        SaveCategories();
    }

    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
        if (CategoriesGrid.SelectedItem is not CategoryRow selected)
        {
            System.Windows.MessageBox.Show(
                "Select a category to edit.",
                "Edit Category",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        var dialog = new AddEditCategoryDialog("Edit Category", selected.Name, selected.Extensions)
        {
            Owner = this
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        if (!string.Equals(selected.Name, dialog.CategoryName, StringComparison.OrdinalIgnoreCase) &&
            Categories.Any(c => c.Name.Equals(dialog.CategoryName, StringComparison.OrdinalIgnoreCase)))
        {
            System.Windows.MessageBox.Show(
                "Category names must be unique.",
                "Validation Error",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        var extensions = ParseExtensions(dialog.ExtensionsText);
        if (extensions is null)
        {
            return;
        }

        selected.Name = dialog.CategoryName;
        selected.Extensions = string.Join(", ", extensions);
        CategoriesGrid.Items.Refresh();
        SaveCategories();
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (CategoriesGrid.SelectedItem is not CategoryRow selected)
        {
            System.Windows.MessageBox.Show(
                "Select a category to delete.",
                "Delete Category",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        var confirm = System.Windows.MessageBox.Show(
            $"Delete '{selected.Name}'?",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes)
        {
            return;
        }

        Categories.Remove(selected);
        SaveCategories();
    }

    private void ResetButton_Click(object sender, RoutedEventArgs e)
    {
        var confirm = System.Windows.MessageBox.Show(
            "Reset categories to defaults?",
            "Reset Categories",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes)
        {
            return;
        }

        _config = _configurationService.CreateDefaultConfiguration();
        _configurationService.SaveConfiguration(_config);
        LoadCategories();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void SaveCategories()
    {
        var updated = new List<FileCategory>();
        foreach (var row in Categories)
        {
            var parsed = ParseExtensions(row.Extensions, row.Name, showErrors: true);
            if (parsed is null)
            {
                return;
            }

            updated.Add(new FileCategory
            {
                Name = row.Name,
                Extensions = parsed.ToList()
            });
        }

        _config.Categories = updated;
        _configurationService.SaveConfiguration(_config);
    }

    private static string[]? ParseExtensions(string input, string? categoryName = null, bool showErrors = true)
    {
        var parts = input
            .Split(new[] { ',', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(part => part.Trim().TrimStart('.'))
            .Where(part => !string.IsNullOrWhiteSpace(part))
            .ToArray();

        if (parts.Length == 0)
        {
            if (showErrors)
            {
                var label = string.IsNullOrWhiteSpace(categoryName) ? "category" : $"category '{categoryName}'";
                System.Windows.MessageBox.Show(
                    $"Provide at least one extension for {label}.",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            return null;
        }

        foreach (var part in parts)
        {
            if (!ExtensionPattern.IsMatch(part))
            {
                if (showErrors)
                {
                    var label = string.IsNullOrWhiteSpace(categoryName) ? "category" : $"category '{categoryName}'";
                    System.Windows.MessageBox.Show(
                        $"Invalid extension in {label}: {part}",
                        "Validation Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
                return null;
            }
        }

        return parts;
    }
}
