
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;
using ProjectScheduler.Models;
using ProjectScheduler.Services;
using System;

namespace ProjectScheduler.Pages;

public sealed partial class HomePage : Page
{
    private readonly PreWorkChecklistService _checklistService = new();

    public HomePage()
    {
        InitializeComponent();

        Loaded += HomePage_Loaded;
        Unloaded += HomePage_Unloaded;
    }

    private async void HomePage_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        try
        {
            // Load the checklist only once per application session.
            if (!ChecklistAccessState.IsChecklistLoaded)
            {
                var items = await _checklistService.LoadAsync();

                ChecklistAccessState.ChecklistItems.Clear();

                foreach (var item in items)
                {
                    ChecklistAccessState.ChecklistItems.Add(item);
                }

                ChecklistAccessState.IsChecklistLoaded = true;
            }

            // Display the shared collection.
            HomeChecklistItems.ItemsSource =
                ChecklistAccessState.ChecklistItems;

            // Listen for changes to checklist completion.
            foreach (var item in ChecklistAccessState.ChecklistItems)
            {
                item.PropertyChanged -= ChecklistItem_PropertyChanged;
                item.PropertyChanged += ChecklistItem_PropertyChanged;
            }

            ChecklistAccessState.UpdateAccess();

            ChecklistAccessState.AccessChanged += AccessChanged;
            UpdateStartScheduleButton();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"Could not load checklist: {ex.Message}"
            );
        }
    }

    private void ChecklistItem_PropertyChanged(
        object? sender,
        PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PreWorkItem.IsCompleted))
        {
            ChecklistAccessState.UpdateAccess();
        }
    }

    private void AccessChanged()
    {
        UpdateStartScheduleButton();
    }

    private void UpdateStartScheduleButton()
    {
        StartScheduleButton.IsEnabled =
            ChecklistAccessState.IsUnlocked;
    }

    private void StartScheduleButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (!ChecklistAccessState.IsUnlocked)
            return;

        MainWindow.Current?.NavigateToSchedules();
    }

    private void HomePage_Unloaded(
        object sender,
        RoutedEventArgs e)
    {
        ChecklistAccessState.AccessChanged -= AccessChanged;

        foreach (var item in ChecklistAccessState.ChecklistItems)
        {
            item.PropertyChanged -= ChecklistItem_PropertyChanged;
        }
    }
}
