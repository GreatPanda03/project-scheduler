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
using Windows.Gaming.Input;
using Windows.ApplicationModel.Appointments;

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
        
        private readonly ObservableCollection<TaskListEntry> _filteredProjectTasks = new();


        // [[ CONSTRUCTOR ]] //

        public ProjectsPage()
        {
            InitializeComponent();

            ProjectsListView.ItemsSource = _filteredProjects;
            ProjectResourcesListView.ItemsSource = _filteredProjectResources;
            TasksListView.ItemsSource = _filteredProjectTasks;

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
            RefreshTasks(project);

            RefreshTaskPlaceholder(project);

            ProjectPlaceholderPanel.Visibility = Visibility.Collapsed;
            ProjectDetailsPanel.Visibility = Visibility.Visible;
        }

        private void ShowProjectPlaceholder()
        {
            ProjectDetailsPanel.Visibility = Visibility.Collapsed;
            ProjectPlaceholderPanel.Visibility = Visibility.Visible;

            _projectResources.Clear();
            _filteredProjectResources.Clear();
            _filteredProjectTasks.Clear();

            TasksListViewEmpty.Visibility = Visibility.Visible;
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
            var filtered = _projectResources
                .OrderBy(resource => resource.Name)
                .ToList();

            _filteredProjectResources.Clear();

            foreach (var resource in filtered)
            {
                _filteredProjectResources.Add(resource);
            }
        }

        private async void ProjectResourcesListView_SelectedChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            // Hide the action buttons for every visible Resource
            foreach (var resource in _filteredProjectResources)
            {
                var container = ProjectResourcesListView.ContainerFromItem(resource)
                    as DependencyObject;

                if (container == null)
                {
                    continue;
                }

                var actionsPanel = FindNamedElement<StackPanel>(
                    container,
                    "ResourceActionsPanel");

                if (actionsPanel != null)
                {
                    actionsPanel.Visibility = Visibility.Collapsed;
                }
            }

            // Display the action buttons for the selected Resource
            // * TODO: combine this with the above if statement if possible
            if (ProjectResourcesListView.SelectedItem is ResourceItem selectedResource)
            {
                var container = ProjectResourcesListView.ContainerFromItem(selectedResource)
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

        private void OpenProjectResourceButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            Debug.WriteLine("Resource access via project is a WIP");
        }

        private void RemoveProjectResourceButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (ProjectsListView.SelectedItem is not ProjectItem project)
            {
                return;
            }

            if (sender is not Button button ||
                button.DataContext is not ResourceItem resource)
            {
                return;
            }

            // Remove the Resource's association from the Project
            ProjectResourcesListView.SelectedItem = null;
            
            project.ResourceIds.Remove(resource.Id);

            _projectResources.Remove(resource);

            RefreshProjectResources(project);
        }


        // [[ PROJECT TASK LIST ]] //
        private async Task<(string Name, string Description)?>
            ShowTaskEditorDialogueAsync(string title, TaskItem? existingTask = null)
        {
            TextBox nameTextBox = new()
            {
                Header = "Name",
                PlaceholderText = "Enter a task name",
                Text = existingTask?.Name ?? string.Empty
            };

            TextBox descriptionTextBox = new()
            {
                Header = "Description",
                PlaceholderText = "Enter a task description",
                Text = existingTask?.Description ?? string.Empty,
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                MinHeight = 80
            };

            StackPanel content = new()
            {
                Spacing = 12,
                MinWidth = 300
            };

            content.Children.Add(nameTextBox);
            content.Children.Add(descriptionTextBox);

            ContentDialog dialogue = new()
            {
                Title = title,
                Content = content,
                PrimaryButtonText = "Save",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = XamlRoot
            };

            // Awaiting user input
            var result = await dialogue.ShowAsync();

            if (result != ContentDialogResult.Primary)
            {
                return null;
            }

            string name = nameTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                await ShowErrorDialogue("Please enter a task name.");
                return null;
            }

            return (
                name, 
                descriptionTextBox.Text.Trim()
            );
        }

        private void TaskFilters_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (ProjectsListView.SelectedItem is ProjectItem project)
            {
                RefreshTasks(project);
            }
        }

        private async void AddTaskButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (ProjectsListView.SelectedItem is not ProjectItem project)
            {
                return;
            }

            var details = await ShowTaskEditorDialogueAsync("Add Task");

            if (details == null)
            {
                return;
            }

            if (project.Tasks.Any(task =>
                task.Name.Equals(details.Value.Name, StringComparison.OrdinalIgnoreCase)))
            {
                await ShowErrorDialogue(
                        "A task with this name already exists for this project.");

                return;
            }

            TaskItem newTask = new()
            {
                Name = details.Value.Name,
                Description = details.Value.Description
            };

            project.Tasks.Add(newTask);

            RefreshTasks(project);
        }

        private async void AddSubtaskButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (ProjectsListView.SelectedItem is not ProjectItem project)
            {
                return;
            }

            if (TasksListView.SelectedItem is not TaskListEntry selectedEntry)
            {
                await ShowErrorDialogue(
                    "Please select a task before adding a subtask.");

                return;
            }

            TaskItem parentTask = selectedEntry.Task;

            var details = await ShowTaskEditorDialogueAsync("Add Subtask");

            if (details == null)
            {
                return;
            }

            if (parentTask.Subtasks.Any(task =>
                task.Name.Equals(details.Value.Name, StringComparison.OrdinalIgnoreCase)))
            {
                await ShowErrorDialogue(
                    "A subtask with this name already exists at this level");

                return;
            }

            TaskItem newSubtask = new()
            {
                Name = details.Value.Name,
                Description = details.Value.Description
            };

            parentTask.AddSubtask(newSubtask);

            RefreshTasks(project);
        }

        private async void EditTaskButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (ProjectsListView.SelectedItem is not ProjectItem project)
            {
                return;
            }

            if (TasksListView.SelectedItem is not TaskListEntry selectedEntry)
            {
                await ShowErrorDialogue(
                    "Please select a task to edit.");

                return;
            }

            TaskItem task = selectedEntry.Task;

            var details = await ShowTaskEditorDialogueAsync("Edit Task", task);

            if (details == null)
            {
                return;
            }

            var siblingTasks = FindParentCollection(project.Tasks, task);

            if (siblingTasks == null)
            {
                return;
            }

            if (siblingTasks.Any(other =>
                other.Id != task.Id && 
                other.Name.Equals(details.Value.Name, StringComparison.OrdinalIgnoreCase)))
            {
                await ShowErrorDialogue(
                    "A task with this name already exists at this level.");

                return;
            }

            task.Name = details.Value.Name;
            task.Description = details.Value.Description;
        }

        private async void DeleteTaskButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (ProjectsListView.SelectedItem is not ProjectItem project)
            {
                return;
            }

            if (TasksListView.SelectedItem is not TaskListEntry selectedEntry)
            {
                await ShowErrorDialogue(
                    "Please select a task to delete.");

                return;
            }

            TaskItem task = selectedEntry.Task;

            ContentDialog confirmation = new()
            {
                Title = "Delete Task",
                Content = $"Are you sure you want to delete \"{task.Name}\" and its subtasks?",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = XamlRoot
            };

            var result = await confirmation.ShowAsync();

            if (result != ContentDialogResult.Primary)
            {
                return;
            }

            var parentCollection = FindParentCollection(project.Tasks, task);

            parentCollection?.Remove(task);

            RefreshTasks(project);
        }

        private void TaskCompletionCheckBox_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (ProjectsListView.SelectedItem is ProjectItem project)
            {
                RefreshTasks(project);
            }
        }

        private ObservableCollection<TaskItem>? FindParentCollection(
            ObservableCollection<TaskItem> tasks,
            TaskItem target)
        {
            foreach (var task in tasks)
            {
                if (task.Id == target.Id)
                {
                    return tasks;
                }

                var result = FindParentCollection(task.Subtasks, target);

                if (result != null)
                {
                    return result;
                }
            }


            return null;
        }

        private void RefreshTasks(ProjectItem project)
        {
            _filteredProjectTasks.Clear();

            var allTasks = new List<TaskListEntry>();

            foreach (var task in project.Tasks)
            {
                AddTaskAndSubtasksToList(task, 0, allTasks);
            }

            int completionFilter = TaskCompletionFilterComboBox.SelectedIndex;

            IEnumerable<TaskListEntry> filteredTasks = allTasks;

            // Completion filter (All = 0, Complete, Incomplete)
            if (completionFilter == 1)
            {
                filteredTasks = filteredTasks.Where(
                    entry => entry.Task.IsCompleted);
            }
            else if (completionFilter == 2)
            {
                filteredTasks = filteredTasks.Where(
                    entry => !entry.Task.IsCompleted);
            }

            // Date sort: oldest / newest
            if (TaskDateSortComboBox.SelectedIndex == 1)
            {
                filteredTasks = filteredTasks
                    .OrderByDescending(entry => entry.Task.CreatedDate);
            }
            else
            {
                filteredTasks = filteredTasks
                    .OrderBy(entry => entry.Task.CreatedDate);
            }

            // Refreshing the list
            foreach (var entry in filteredTasks)
            {
                _filteredProjectTasks.Add(entry);
            }

            RefreshTaskPlaceholder(project);
        }

        private void AddTaskAndSubtasksToList(
            TaskItem task,
            int depth,
            List<TaskListEntry> entries)
        {
            entries.Add(new TaskListEntry(task, depth));

            foreach (var subtask in task.Subtasks)
            {
                AddTaskAndSubtasksToList(
                    subtask,
                    depth + 1,
                    entries);
            }
        }

        private void RefreshTaskPlaceholder(ProjectItem project)
        {
            bool hasTasks = project.Tasks.Count > 0;

            TasksListViewEmpty.Visibility =
                hasTasks ? Visibility.Collapsed : Visibility.Visible;
        }


        // [[ AUXILIARY: FIND ELEMENT IN ITEM ]] //
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
                {
                    return element;
                }

                var result = FindNamedElement<T>(child, elementName);

                if (result != null)
                {
                    return result;
                }
            }

            return null;
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
