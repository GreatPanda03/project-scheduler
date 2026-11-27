using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using ProjectScheduler.Interfaces;
using ProjectScheduler.Models;
using ProjectScheduler.Services;
using ProjectScheduler.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectScheduler.Controls
{
    public sealed partial class ResourceAssignView : UserControl
    {
        private ResourceAssignViewModel ViewModel; // changes according to Target
        private ObservableCollection<ResourceItem> _assignedResources;

        public ResourceAssignView()
        {
            InitializeComponent();

            ViewModel = new();

            _assignedResources = new();
        }

        // Title
        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                nameof(Title),
                typeof(string),
                typeof(ResourceAssignView),
                new PropertyMetadata("Resources"));

        // Target to Assign Resources
        public IHasResources? Target
        {
            get => (IHasResources?)GetValue(TargetProperty);
            set => SetValue(TargetProperty, value);
        }

        public static readonly DependencyProperty TargetProperty =
            DependencyProperty.Register(
                nameof(Target),
                typeof(IHasResources),
                typeof(ResourceAssignView),
                new PropertyMetadata(null, OnTargetChanged));

        public static void OnTargetChanged(
            DependencyObject sender,
            DependencyPropertyChangedEventArgs e)
        {
            if (sender is ResourceAssignView control)
            {
                control.ViewModel.Target = (IHasResources)e.NewValue;
                control.RefreshItems();
            }
        }

        public void RefreshItems()
        {
            _assignedResources.Clear();

            if (ViewModel.Target is null) return;

            List<ResourceItem> resources = ViewModel.GetTargetResources();
            
            foreach (ResourceItem resource in resources)
                _assignedResources.Add(resource);

            var phPanel = (StackPanel)FindName("PlaceholderPanel");
            var listPanel = (ListView)FindName("ListPanel");

            phPanel.Visibility = _assignedResources.Count == 0
                ? Visibility.Visible : Visibility.Collapsed;

            listPanel.Visibility = _assignedResources.Count == 0
                ? Visibility.Collapsed : Visibility.Visible;
        }



        // [[ EVENTS ]] //

        // Add Resource
        private async void AddResourceButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (ViewModel is null) return;
            if (ViewModel.Target is null) return;

            ResourceItem? resource = await PromptSelectResource();

            if (resource is null) return;

            bool added = ViewModel.AddResource(resource.Id);
            if (!added) return;

            RefreshItems();
        }

        // Open Resource
        private void OpenResourceButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is FrameworkElement element &&
                element.DataContext is ResourceItem resource)
            {
                new ResourceOpener().OpenResource(resource);
            }
        }

        // Remove Resource
        private void RemoveResourceButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (ViewModel == null ||
                sender is not FrameworkElement element ||
                element.DataContext is not ResourceItem resource)
            {
                return;
            }

            bool removed = ViewModel.RemoveResource(resource.Id);
            if (!removed) return;

            RefreshItems();
        }



        // [[ RESOURCE ENTRY ACTIONS ]] //

        private void ResourcesListView_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (sender is not ListView) return;

            ListView listView = (ListView)sender;

            // Hide action buttons for all resources
            foreach (var resource in listView.Items)
            {
                var container = listView.ContainerFromItem(resource)
                    as DependencyObject;

                if (container == null) return;

                var actionsPanel = FindNamedElement<StackPanel>(
                    container,
                    "ResourceActionsPanel");

                if (actionsPanel != null)
                {
                    actionsPanel.Visibility = Visibility.Collapsed;
                }
            }

            // Show action buttons for the selected resource
            if (listView.SelectedItem is ResourceItem selectedResource)
            {
                var container = listView.ContainerFromItem(selectedResource)
                    as DependencyObject;

                if (container != null)
                {
                    var actionsPanel = FindNamedElement<StackPanel>(
                        container,
                        "ResourceActionsPanel");

                    if (actionsPanel != null)
                    {
                        actionsPanel.Visibility = Visibility.Visible;
                    }
                }
            }
        }



        // [[ DIALOGUES ]] //

        private async Task<ResourceItem?> PromptSelectResource()
        {
            if (Target is null) return null;

            // Create the Resource selection interface
            TextBox searchBox = new()
            {
                PlaceholderText = "Search resources..."
            };

            ListView resourceListView = new()
            {
                SelectionMode = ListViewSelectionMode.Single,
                MaxHeight = 300,
                ItemTemplate = GetResourceTemplate()
            };

            void RefreshPickerList()
            {
                string search = searchBox.Text.Trim();

                resourceListView.Items.Clear();

                var filteredResources = ViewModel.GetAllResources()
                    .Where(resource =>
                        !Target.ResourceIds.Contains(resource.Id) &&
                        resource.Name.Contains(
                            search,
                            StringComparison.OrdinalIgnoreCase))
                    .OrderBy(resource => resource.Name);

                foreach (ResourceItem resource in filteredResources)
                {
                    resourceListView.Items.Add(resource);
                }
            }

            searchBox.TextChanged += (s, args) =>
            {
                RefreshPickerList();
            };

            RefreshPickerList();

            Grid content = new()
            {
                RowSpacing = 12,
                MinWidth = 300
            };

            content.RowDefinitions.Add(
                new RowDefinition { Height = GridLength.Auto });

            content.RowDefinitions.Add(
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            Grid.SetRow(searchBox, 0);
            Grid.SetRow(resourceListView, 1);

            content.Children.Add(searchBox);
            content.Children.Add(resourceListView);

            ContentDialog dialogue = new()
            {
                Title = "Add Resource",
                Content = content,
                PrimaryButtonText = "Add",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = XamlRoot
            };

            // Await user selection
            var result = await dialogue.ShowAsync();

            if (result != ContentDialogResult.Primary)
                return null;

            return resourceListView.SelectedItem as ResourceItem;
        }



        // [[ AUXILIARY METHODS ]] //
        private T? FindNamedElement<T>(
            DependencyObject parent,
            string elementName)
            where T : FrameworkElement
        {
            int childCount = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T element && element.Name == elementName)
                    return element;

                var result = FindNamedElement<T>(child, elementName);

                if (result != null)
                    return result;
            }

            return null;
        }

        public DataTemplate GetResourceTemplate()
        {
            string xaml = """
                <DataTemplate
                    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">

                    <StackPanel
                        Orientation="Horizontal"
                        Spacing="8">

                        <FontIcon
                            Glyph="{Binding IconGlyph}"
                            VerticalAlignment="Center"/>

                        <TextBlock
                            Text="{Binding Name}"
                            VerticalAlignment="Center"/>

                    </StackPanel>

                </DataTemplate>
                """;

            return (DataTemplate)XamlReader.Load(xaml);
        }
    }
}