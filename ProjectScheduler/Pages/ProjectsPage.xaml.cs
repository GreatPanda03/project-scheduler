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
        private readonly ProjectService _projectService = new();
        private bool _isLoadingProjects;
        private readonly ObservableCollection<ProjectItem> _projects = new();
        private readonly ObservableCollection<ProjectItem> _filteredProjects = new();

        private readonly ResourceService _resourceService = new();
        private readonly List<ResourceItem> _allResources = new();
        private readonly ObservableCollection<ResourceItem> _projectResources = new();
        private readonly ObservableCollection<ResourceItem> _filteredProjectResources = new();
        
        private readonly ObservableCollection<TaskListEntry> _filteredProjectTasks = new();
        private readonly HashSet<Guid> _collapsedTaskIds = new();


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
            await LoadProjectsAsync();
            await LoadResourcesAsync();
        }

        private async Task LoadProjectsAsync()
        {
            try
            {
                _isLoadingProjects = true;

                var savedProjects = await _projectService.LoadAsync();

                _projects.Clear();

                foreach (var project in savedProjects)
                {
                    _projects.Add(project);
                }

                RefreshProjectList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Could not load projects: {ex.Message}");
            }
            finally
            {
                _isLoadingProjects = false;
            }
        }

        private async Task SaveProjectsAsync()
        {
            if (_isLoadingProjects)
            {
                return;
            }

            try
            {
                await _projectService.SaveAsync(_projects);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Could not save projects: {ex.Message}");

                await ShowErrorDialogue(
                    "Could not save projects. Your latest changes may not be preserved.");
            }
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
                PlaceholderText = "Enter a project description",
                MinHeight = 80
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

            await SaveProjectsAsync();

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

            // Update the visibility of the project list and placeholders
            ProjectsListViewEmpty.Visibility =
                _projects.Count == 0
                ? Visibility.Visible
                : Visibility.Collapsed;

            ProjectsListView.Visibility =
                _filteredProjects.Count > 0
                ? Visibility.Visible
                : Visibility.Collapsed;

            ProjectsListViewNoResults.Visibility =
                _projects.Count > 0 && _filteredProjects.Count == 0
                ? Visibility.Visible
                : Visibility.Collapsed;
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
            ProjectResourcesListView.SelectedItem = null;
            TasksListView.SelectedItem = null;

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

            await SaveProjectsAsync();

            RefreshProjectList();

            ProjectsListView.SelectedItem =
                _filteredProjects.FirstOrDefault(
                    item => item.Id == project.Id);
        }

        private async void DeleteProjectButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (ProjectsListView.SelectedItem is not ProjectItem project)
            {
                return;
            }

            // Ask the user to confirm deletion
            ContentDialog dialogue = new()
            {
                Title = "Delete Project",
                Content = $"Are you sure you want to delete \"{project.Name}\"? " +
                    "This will also delete all its tasks and subtasks.",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = XamlRoot
            };

            var result = await dialogue.ShowAsync();

            if (result != ContentDialogResult.Primary)
            {
                return;
            }

            _projects.Remove(project);

            await SaveProjectsAsync();

            RefreshProjectList();

            ProjectsListView.SelectedItem = null;
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

        private async void AddProjectResourceButton_Click(
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

            await SaveProjectsAsync();

            RefreshProjectResources(project);
        }

        private void OpenProjectResourceButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            Debug.WriteLine("Resource access via project is a WIP");
        }

        private async void RemoveProjectResourceButton_Click(
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

            await SaveProjectsAsync();

            _projectResources.Remove(resource);

            RefreshProjectResources(project);
        }


        // [[ PROJECT TASK LIST ]] //
        private void ToggleTaskExpansionButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is not Button button ||
                button.DataContext is not TaskListEntry entry)
            {
                return;
            }

            Guid taskId = entry.Task.Id;

            // Toggle the task's collapsed state (find & add / remove)
            if (_collapsedTaskIds.Contains(taskId))
            {
                _collapsedTaskIds.Remove(taskId);
            }
            else
            {
                _collapsedTaskIds.Add(taskId);
            }

            // Refresh the visible task hierarchy
            if (ProjectsListView.SelectedItem is ProjectItem project)
            {
                RefreshTasks(project);
            }
        }

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

        private void TasksListView_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            // Hide the action buttons for every visible task
            foreach (var taskEntry in _filteredProjectTasks)
            {
                var container = TasksListView.ContainerFromItem(taskEntry)
                    as DependencyObject;

                if (container == null)
                {
                    continue;
                }

                var actionsPanel = FindNamedElement<StackPanel>(
                    container,
                    "TaskActionsPanel");

                if (actionsPanel != null)
                {
                    actionsPanel.Visibility = Visibility.Collapsed;
                }
            }

            // Display the action buttons for the selected task
            if (TasksListView.SelectedItem is TaskListEntry selectedEntry)
            {
                var container = TasksListView.ContainerFromItem(selectedEntry)
                    as DependencyObject;

                if (container != null)
                {
                    var actionsPanel = FindNamedElement<StackPanel>(
                        container,
                        "TaskActionsPanel");
                    
                    if (actionsPanel != null)
                    {
                        actionsPanel.Visibility = Visibility.Visible;
                    }
                }
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
                task.Name.Equals(
                    details.Value.Name, 
                    StringComparison.OrdinalIgnoreCase)))
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

            await SaveProjectsAsync();

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

            // Retrieving the task to add a subtask to
            TaskListEntry? selectedEntry = GetTaskEntryFromButton(sender);

            if (selectedEntry == null)
            {
                return;
            }

            TaskItem parentTask = selectedEntry.Task;

            // Prompting for the new subtask's name
            var details = await ShowTaskEditorDialogueAsync("Add Subtask");

            if (details == null)
            {
                return;
            }

            // Validating the new details (local uniqueness)
            if (parentTask.Subtasks.Any(task =>
                task.Name.Equals(
                    details.Value.Name,
                    StringComparison.OrdinalIgnoreCase)))
            {
                await ShowErrorDialogue(
                    "A subtask with this name already exists at this level");

                return;
            }

            // Creating the new subtask under the target task
            TaskItem newSubtask = new()
            {
                Name = details.Value.Name,
                Description = details.Value.Description
            };

            parentTask.AddSubtask(newSubtask);

            _collapsedTaskIds.Remove(parentTask.Id); // ie. visible

            await SaveProjectsAsync();

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

            // Retrieving the task associated with the button
            TaskListEntry? selectedEntry = GetTaskEntryFromButton(sender);

            if (selectedEntry == null)
            {
                return;
            }

            TaskItem task = selectedEntry.Task;

            // Prompting for updated task details
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

            // Validating the new task details (local uniqueness)
            if (siblingTasks.Any(other =>
                other.Id != task.Id && 
                other.Name.Equals(
                    details.Value.Name,
                    StringComparison.OrdinalIgnoreCase)))
            {
                await ShowErrorDialogue(
                    "A task with this name already exists at this level.");

                return;
            }

            task.Name = details.Value.Name;
            task.Description = details.Value.Description;

            await SaveProjectsAsync();

            RefreshTasks(project);
        }

        private async void DeleteTaskButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (ProjectsListView.SelectedItem is not ProjectItem project)
            {
                return;
            }

            // Retrieving the button's associated task
            TaskListEntry? selectedEntry = GetTaskEntryFromButton(sender);

            if (selectedEntry == null)
            {
                return;
            }

            TaskItem task = selectedEntry.Task;

            // Prompting the user to confirm deletion
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

            // Deleting the task and its subtasks if confirmed
            var parentCollection = FindParentCollection(project.Tasks, task);

            parentCollection?.Remove(task);

            await SaveProjectsAsync();

            RefreshTasks(project);
        }

        private async void TaskCompletionCheckBox_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (ProjectsListView.SelectedItem is not ProjectItem project)
            {
                return;
            }

            await SaveProjectsAsync();

            RefreshTasks(project);
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
            // Remember the selected task to restore its selection
            Guid? selectedTaskId =
                (TasksListView.SelectedItem as TaskListEntry)?.Task.Id;

            _filteredProjectTasks.Clear();

            var allTasks = new List<TaskListEntry>();

            bool descending = TaskDateSortComboBox.SelectedIndex == 1;

            // Build the hierarchy and sort tasks at each level
            AddTaskAndSubtasksToList(
                project.Tasks, 0, allTasks, descending);

            int completionFilter = TaskCompletionFilterComboBox.SelectedIndex;

            IEnumerable <TaskListEntry> filteredTasks = allTasks;

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

            // * NOTE: Date sorting done in AddTaskAndSubtasksToList()

            // Refreshing the list
            foreach (var entry in filteredTasks)
            {
                _filteredProjectTasks.Add(entry);
            }

            // Restore the selection if the task is still visible
            if (selectedTaskId.HasValue)
            {
                TasksListView.SelectedItem =
                    _filteredProjectTasks.FirstOrDefault(
                        entry => entry.Task.Id == selectedTaskId.Value);
            }

            RefreshTaskPlaceholder(project);
        }

        private void AddTaskAndSubtasksToList(
            IEnumerable<TaskItem> tasks,
            int depth,
            List<TaskListEntry> entries,
            bool descending)
        {
            // Sort only tasks that share the same parent
            var orderedTasks = descending
                ? tasks.OrderByDescending(task => task.CreatedDate)
                : tasks.OrderBy(task => task.CreatedDate);

            foreach (var task in orderedTasks)
            {
                bool isCollapsed = _collapsedTaskIds.Contains(task.Id);

                // Create the entry with the appropriate expansion glyph
                TaskListEntry entry = new(task, depth)
                {
                    ExpandGlyph = isCollapsed ? "\uE76C" : "\uE70D"
                };

                // Add the parent task first, then its subtasks (depth-first traversal)
                // * only add Subtasks if the parent is expanded
                entries.Add(entry);

                if (!isCollapsed) {
                    AddTaskAndSubtasksToList(
                        task.Subtasks,
                        depth + 1,
                        entries,
                        descending);
                }
            }
        }

        private void RefreshTaskPlaceholder(ProjectItem project)
        {
            bool hasTasks = project.Tasks.Count > 0;

            TasksListViewEmpty.Visibility =
                hasTasks ? Visibility.Collapsed : Visibility.Visible;
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

        private TaskListEntry? GetTaskEntryFromButton(object sender)
        {
            if (sender is Button button &&
                button.DataContext is TaskListEntry entry)
            {
                TasksListView.SelectedItem = entry;
                return entry;
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
