using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ProjectScheduler.Enums;
using ProjectScheduler.Models;
using System;

namespace ProjectScheduler.Controls;

public sealed partial class ResourceDetailsView : UserControl
{
    public ResourceDetailsView()
    {
        InitializeComponent();

        ResourceTypeComboBox.ItemsSource =
            Enum.GetValues<ResourceType>();
    }

    // Selected Resource
    public ResourceItem? Resource
    {
        get => (ResourceItem?)GetValue(ResourceProperty);
        set => SetValue(ResourceProperty, value);
    }

    public static DependencyProperty ResourceProperty =
        DependencyProperty.Register(
            nameof(Resource),
            typeof(ResourceItem),
            typeof(ResourceDetailsView),
            new PropertyMetadata(null, OnResourceChanged));

    // Update the displayed resource
    private static void OnResourceChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e)
    {
        var control = (ResourceDetailsView)d;

        bool hasResource = e.NewValue is ResourceItem;

        control.PlaceholderPanel.Visibility =
            hasResource ? Visibility.Collapsed : Visibility.Visible;

        control.DetailsPanel.Visibility =
            hasResource ? Visibility.Visible : Visibility.Collapsed;

        control.DetailsPanel.DataContext = e.NewValue;
    }

    // Open event
    public event EventHandler? OpenRequested;

    private void OpenButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        OpenRequested?.Invoke(this, EventArgs.Empty);
    }

    // Save event
    public event EventHandler? SaveRequested;

    private void SaveButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        SaveRequested?.Invoke(this, EventArgs.Empty);
    }

    // Delete event
    public event EventHandler? DeleteRequested;

    private void DeleteButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        DeleteRequested?.Invoke(this, EventArgs.Empty);
    }
}
