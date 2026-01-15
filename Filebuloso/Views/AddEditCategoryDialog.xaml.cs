using System;
using System.Text.RegularExpressions;
using System.Windows;

namespace Filebuloso.Views;

/// <summary>
/// Interaction logic for AddEditCategoryDialog.xaml
/// </summary>
public partial class AddEditCategoryDialog : Window
{
    private static readonly Regex NamePattern = new("^[A-Za-z0-9_]+$");

    public string CategoryName => NameTextBox.Text.Trim();
    public string ExtensionsText => ExtensionsTextBox.Text.Trim();

    public AddEditCategoryDialog(string title, string? name = null, string? extensions = null)
    {
        InitializeComponent();
        Title = title;
        NameTextBox.Text = name ?? string.Empty;
        ExtensionsTextBox.Text = extensions ?? string.Empty;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(CategoryName) || !NamePattern.IsMatch(CategoryName))
        {
            System.Windows.MessageBox.Show(
                "Category name must use letters, numbers, and underscores only.",
                "Validation Error",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(ExtensionsText))
        {
            System.Windows.MessageBox.Show(
                "Provide at least one extension.",
                "Validation Error",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
