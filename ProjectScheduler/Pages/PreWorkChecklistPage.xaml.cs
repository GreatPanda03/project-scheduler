
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using ProjectScheduler.Models;
using ProjectScheduler.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.System;

namespace ProjectScheduler.Pages;

public sealed partial class PreWorkChecklistPage : Page
{
    private readonly PreWorkChecklistService _checklistService = new();

    private ObservableCollection<PreWorkItem> _items =>
        ChecklistAccessState.ChecklistItems;

    public PreWorkChecklistPage()
    {
        InitializeComponent();

        ItemsListView.ItemsSource = _items;

        Loaded += PreWorkChecklistPage_Loaded;
    }

    // Load existing items when the page is opened.
    private async void PreWorkChecklistPage_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        if (!ChecklistAccessState.IsChecklistLoaded)
        {
            var items = await _checklistService.LoadAsync();

            _items.Clear();

            foreach (var item in items)
            {
                _items.Add(item);
            }
        }

        ChecklistAccessState.IsChecklistLoaded = true;
    }

    // Add a new item.
    private void AddItemButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        string title = NewItemTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(title))
        {
            return;
        }

        _items.Add(new PreWorkItem
        {
            Title = title
        });

        NewItemTextBox.Text = string.Empty;
        StatusText.Text = "You have unsaved changes.";
    }

    // Allow Enter to add an item.
    private void NewItemTextBox_KeyDown(
        object sender,
        KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter)
        {
            AddItemButton_Click(sender, e);
        }
    }

    // Edit an existing item.
    private async void EditItemButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is not FrameworkElement element ||
            element.DataContext is not PreWorkItem item)
        {
            return;
        }

        var editor = new TextBox
        {
            Text = item.Title,
            PlaceholderText = "Enter a checklist item..."
        };

        var dialog = new ContentDialog
        {
            Title = "Edit checklist item",
            Content = editor,
            PrimaryButtonText = "Save",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = XamlRoot
        };

        ContentDialogResult result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            string newTitle = editor.Text.Trim();

            if (string.IsNullOrWhiteSpace(newTitle))
            {
                return;
            }

            item.Title = newTitle;

            // Notify the interface that the item has changed.
            StatusText.Text = "You have unsaved changes.";
        }
    }

    // Remove an existing item.
    private void RemoveItemButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is not FrameworkElement element ||
            element.DataContext is not PreWorkItem item)
        {
            return;
        }

        _items.Remove(item);

        StatusText.Text = "You have unsaved changes.";
    }

    // Save the checklist.
    private async void SaveButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        SaveButton.IsEnabled = false;

        try
        {
            await _checklistService.SaveAsync(_items.ToList());

            StatusText.Text = "Checklist saved successfully!";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Could not save checklist: {ex.Message}";
        }
        finally
        {
            SaveButton.IsEnabled = true;
        }
    }
}
