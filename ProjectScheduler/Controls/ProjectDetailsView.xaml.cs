using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ProjectScheduler.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectScheduler.Controls;

public sealed partial class ProjectDetailsView : UserControl
{
    public ProjectDetailsView()
    {
        InitializeComponent();
    }

    // Selected Project
    public ProjectItem? Project
    {
        get => (ProjectItem?)GetValue(ProjectProperty);
        set => SetValue(ProjectProperty, value);
    }

    public static readonly DependencyProperty ProjectProperty =
        DependencyProperty.Register(
            nameof(Project),
            typeof(ProjectItem),
            typeof(ProjectDetailsView),
            new PropertyMetadata(null, OnProjectChanged));

    // Project Resources
    public IEnumerable<ResourceItem>? ResourceItems
    {
        get => (IEnumerable<ResourceItem>?)GetValue(ResourceItemsProperty);
        set => SetValue(ResourceItemsProperty, value);
    }

    public static readonly DependencyProperty ResourceItemsProperty =
        DependencyProperty.Register(
            nameof(ResourceItems),
            typeof(IEnumerable<ResourceItem>),
            typeof(ProjectDetailsView),
            new PropertyMetadata(null));


    // Update the displayed project
    private static void OnProjectChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e)
    {
        var control = (ProjectDetailsView)d;

        bool hasProject = e.NewValue is ProjectItem;

        control.PlaceholderPanel.Visibility =
            hasProject ? Visibility.Collapsed : Visibility.Visible;

        control.DetailsPanel.Visibility =
            hasProject ? Visibility.Visible : Visibility.Collapsed;

        control.DetailsPanel.DataContext = e.NewValue;
    }

    // Save event
    public event EventHandler? SaveProjectRequested;

    private void SaveProjectButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        SaveProjectRequested?.Invoke(this, EventArgs.Empty);
    }

    // Delete event
    public event EventHandler? DeleteProjectRequested;

    private void DeleteProjectButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        DeleteProjectRequested?.Invoke(this, EventArgs.Empty);
    }
}
