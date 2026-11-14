using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using ProjectScheduler.Models;
using ProjectScheduler.Services;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ProjectScheduler.Pages
{
    public sealed partial class ProjectsPage : Page
    {
        private readonly ObservableCollection<ProjectItem> _projects = new();
        private readonly ObservableCollection<ProjectItem> _filteredProjects = new();

        private readonly ResourceService _resourceService = new();
        private readonly List<ResourceItem> _allResources = new();
        private readonly ObservableCollection<ResourceItem> _projectResources = new();
        private readonly ObservableCollection<ResourceItem> _filteredProjectResources = new();

        // temp task names until task model is implemented
        private readonly ObservableCollection<string> _taskNames = new();


        // [[ CONSTRUCTOR ]] //

        public ProjectsPage()
        {
            InitializeComponent();

            ProjectsListView.ItemsSource = _filteredProjects;
            ProjectResourcesListView.ItemsSource = _filteredProjectResources;
            TasksListView.ItemsSource = _taskNames;

            Loaded += ProjectsPage_Loaded;
        }


        // [[ LOADING RESOURCES ]] //

        private async void ProjectsPage_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            await LoadResourcesAsync();
        }

        private async Task LoadResourcesAsync()
        {
            try
            {
                var savedResources = await _resourceService.LoadAsync();

                _allResources.Clear();
                _allResources.AddRange(savedResources);

                if (ProjectsListView.SelectedItem is ProjectItem project)
                {
                    RefreshProjectResources(project);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"Could not load resources: {ex.Message}");   
            }
        }


        // [[ CREATING PROJECTS ]] //

        private async void AddProjectButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            // Build the dialogue box
            TextBox nameTextBox = new()
            {
                Header = "Name",
                PlaceholderText = "Enter a project name"
            };

            TextBox descriptionTextBox = new()
            {
                Header = "Description",
                PlaceholderText = "Enter a project description"
            };

            NumberBox priorityNumberBox = new()
            {
                Header = "Priority",
                PlaceholderText = "Enter a project priority"
            };

            var dialogueContent = new StackPanel
            {
                Spacing = 12
            };

            dialogueContent.Children.Add(nameTextBox);
            dialogueContent.Children.Add(descriptionTextBox);
            dialogueContent.Children.Add(priorityNumberBox);

            ContentDialog dialogue = new()
            {
                Title = "Add Project",
                Content = dialogueContent,
                PrimaryButtonText = "Add",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = XamlRoot
            };

            // Validate the provided details
            var result = await dialogue.ShowAsync();

            if (result != ContentDialogResult.Primary)
            {
                return;
            }

            string name = nameTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                await ShowErrorDialogue(
                    "Please enter a name for the project.");

                return;
            }

            float priority = (float) priorityNumberBox.Value;

            if (float.IsNaN(priority) || priority < 0)
            {
                await ShowErrorDialogue(
                    "Please enter a numerical priority for the project.");

                return;
            }

            // Create the project and refresh the list
            ProjectItem newProject = new()
            {
                Name = name,
                Description = descriptionTextBox.Text.Trim(),
                Priority = priority
            };

            _projects.Add(newProject);

            RefreshProjectList();

            ProjectsListView.SelectedItem =
                _filteredProjects.FirstOrDefault(
                    project => project.Name == name);
        }


        // [[ SEARCHING PROJECTS ]] //

        private void RefreshProjectList()
        {
            string searchText = SearchProjectsTextBox?.Text ?? string.Empty;

            var filtered = _projects
                .Where(project =>
                    project.Name.Contains(
                        searchText,
                        StringComparison.OrdinalIgnoreCase))
                .OrderBy(project => project.Name)
                .ToList();

            _filteredProjects.Clear();

            foreach (var project in filtered)
            {
                _filteredProjects.Add(project);
            }
        }

        private void SearchProjectsTextBox_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            RefreshProjectList();
        }


        // [[ DISPLAYING SELECTED PROJECT ]] //
        private void ProjectsListView_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (ProjectsListView.SelectedItem is ProjectItem project)
            {
                DisplayProjectDetails(project);
            }
            else
            {
                ShowProjectPlaceholder();
            }
        }

        private void DisplayProjectDetails(ProjectItem project)
        {
            ProjectNameTextBox.Text = project.Name;
            ProjectDescriptionTextBox.Text = project.Description;
            ProjectPriorityNumberBox.Value = project.Priority;
            ProjectNotesTextBox.Text = project.Notes;

            RefreshProjectResources(project);
            _taskNames.Clear();

            RefreshTaskPlaceholder();

            ProjectPlaceholderPanel.Visibility = Visibility.Collapsed;
            ProjectDetailsPanel.Visibility = Visibility.Visible;
        }

        private void ShowProjectPlaceholder()
        {
            ProjectDetailsPanel.Visibility = Visibility.Collapsed;
            ProjectPlaceholderPanel.Visibility = Visibility.Visible;

            _projectResources.Clear();
            _filteredProjectResources.Clear();
            _taskNames.Clear();

            RefreshTaskPlaceholder();
        }


        // [[ SAVING & DELETING PROJECTS ]] //

        private async void SaveProjectButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (ProjectsListView.SelectedItem is not ProjectItem project)
            {
                return;
            }

            string name = ProjectNameTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                await ShowErrorDialogue(
                    "A project must have a name.");

                return;
            }

            float priority = (float) ProjectPriorityNumberBox.Value;

            if (float.IsNaN(priority) || priority < 0)
            {
                await ShowErrorDialogue(
                    "A project must have a positive priority");

                return;
            }

            project.Name = name;
            project.Description = ProjectDescriptionTextBox.Text.Trim();
            project.Priority = priority;
            project.Notes = ProjectNotesTextBox.Text.Trim();

            RefreshProjectList();

            ProjectsListView.SelectedItem =
                _filteredProjects.FirstOrDefault(
                    item => item.Id == project.Id);
        }

        private void DeleteProjectButton_Click(
            object sender,
            RoutedEventArgs e)
        {

        }


        // [[ PROJECT RESOURCE LIST ]] //

        private void RefreshProjectResources(ProjectItem project)
        {
            _projectResources.Clear();

            foreach (Guid resourceId in project.ResourceIds)
            {
                var resource = _allResources.FirstOrDefault(
                    item => item.Id == resourceId);

                if (resource != null)
                {
                    _projectResources.Add(resource);
                }
            }

            RefreshProjectResourcesList();
        }

        private void RefreshProjectResourcesList()
        {
            string searchText =
                ProjectResourceSearchTextBox?.Text ?? string.Empty;

            var filtered = _projectResources
                .Where(resource =>
                    resource.Name.Contains(
                        searchText,
                        StringComparison.OrdinalIgnoreCase))
                .OrderBy(resource => resource.Name)
                .ToList();

            _filteredProjectResources.Clear();

            foreach (var resource in filtered)
            {
                _filteredProjectResources.Add(resource);
            }
        }

        private void ProjectResourceSearchTextBox_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            RefreshProjectResourcesList();
        }

        private async void AddResourceButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (ProjectsListView.SelectedItem is not ProjectItem project)
            {
                return;
            }

            // Create the Resource selection interface
            TextBox searchBox = new()
            {
                PlaceholderText = "Search resources..."
            };

            ListView resourceListView = new()
            {
                SelectionMode = ListViewSelectionMode.Single,
                MaxHeight = 300
            };

            void RefreshPickerList()
            {
                string search = searchBox.Text.Trim();

                resourceListView.Items.Clear();

                var availableResources = _allResources
                    .Where(resource =>
                        !project.ResourceIds.Contains(resource.Id) &&
                        resource.Name.Contains(
                            search,
                            StringComparison.OrdinalIgnoreCase))
                    .OrderBy(resource => resource.Name);

                foreach (var resource in availableResources)
                {
                     StackPanel panel = new()
                    {
                        Orientation = Orientation.Horizontal,
                        Spacing = 8
                    };

                    panel.Children.Add(new FontIcon { Glyph = resource.IconGlyph });
                    panel.Children.Add(new TextBlock { Text = resource.Name });

                    resourceListView.Items.Add(
                        new ListViewItem
                        {
                            Content = panel,
                            Tag = resource
                        });
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
            {
                return;
            }

            if (resourceListView.SelectedItem is not ListViewItem selectedItem ||
                selectedItem.Tag is not ResourceItem selectedResource)
            {
                return;
            }

            // Prevent duplicate associations
            if (project.ResourceIds.Contains(selectedResource.Id))
            {
                return;
            }

            project.ResourceIds.Add(selectedResource.Id);

            RefreshProjectResources(project);
        }

        private void RemoveProjectResourceButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (ProjectsListView.SelectedItem is not ProjectItem project)
            {
                return;
            }

            if (ProjectResourcesListView.SelectedItem is not ResourceItem resource)
            {
                return;
            }

            // Remove the association
            project.ResourceIds.Remove(resource.Id);

            RefreshProjectResources(project);
        }


        // [[ PROJECT TASK LIST ]] //

        private async void AddTaskButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            TextBox taskNameTextBox = new()
            {
                Header = "Task Name",
                PlaceholderText = "Enter a task name"
            };

            ContentDialog dialogue = new()
            {
                Title = "Add Task",
                Content = taskNameTextBox,
                PrimaryButtonText = "Add",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = XamlRoot
            };

            var result = await dialogue.ShowAsync();

            if (result != ContentDialogResult.Primary)
            {
                return;
            }

            string taskName = taskNameTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(taskName))
            {
                await ShowErrorDialogue(
                    "Please enter a name for the task.");

                return;
            }

            // temporary implementation until TaskItem is created
            _taskNames.Add(taskName);

            RefreshTaskPlaceholder();
        }

        private void RefreshTaskPlaceholder()
        {
            NoTasksTextBlock.Visibility =
                _taskNames.Count == 0
                    ? Visibility.Visible
                    : Visibility.Collapsed;
        }


        // [[ ERROR DIALOGUE ]] //
        private async Task ShowErrorDialogue(string message)
        {
            ContentDialog dialogue = new()
            {
                Title = "Project Error",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = XamlRoot
            };

            await dialogue.ShowAsync();
        }
    }
}
