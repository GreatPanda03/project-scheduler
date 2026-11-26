using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using ProjectScheduler.Interfaces;
using ProjectScheduler.Models;
using ProjectScheduler.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectScheduler.Controls
{
    public sealed partial class TaskListView : UserControl
    {
        private TaskViewModel ViewModel { get; }

        public TaskListView()
        {
            InitializeComponent();

            ViewModel = new(Target);
        }


        
        // [[ DEPENDENCY PROPERTIES ]] //

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
                typeof(TaskListView),
                new PropertyMetadata(null));

        // Target
        public IHasTasks Target
        {
            get => (IHasTasks)GetValue(TargetProperty);
            set => SetValue(TargetProperty, value);
        }

        public static readonly DependencyProperty TargetProperty =
            DependencyProperty.Register(
                nameof(Target),
                typeof(IHasTasks),
                typeof(TaskListView),
                new PropertyMetadata(null));

        // Task List
        public IEnumerable<TaskItem>? ItemsSource
        {
            get => (IEnumerable<TaskItem>?)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                nameof(ItemsSource),
                typeof(IEnumerable<TaskItem>),
                typeof(TaskListView),
                new PropertyMetadata(null));



        // [[ ADDING TASKS ]] //

        // Adding Task
        private async void AddTaskButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (ViewModel.SelectedItem is null) return;

            // Prompting for Task details
            var details = await PromptTaskDetails("New Task", null);

            if (!details.HasValue)
                return;

            var (name, description) = details.Value;

            if (ViewModel.SelectedItem.Tasks.Any(task =>
                task.Name.Equals(
                    name,
                    StringComparison.OrdinalIgnoreCase)))
            {
                await ShowErrorDialogue(
                        "A task with this name already exists for this project.");
                return;
            }

            TaskItem task = new()
            {
                Name = name,
                Description = description
            };

            ViewModel.AddTask(task);
        }

        // Add Subtask
        private async void AddSubtaskButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (ViewModel.SelectedItem is not TaskItem task) return;

            // Prompting for the new subtask's name
            var details = await PromptTaskDetails("New Subtask", null);

            if (!details.HasValue)
                return;

            var (name, description) = details.Value;

            // Validating the new details (local uniqueness)
            if (task.Subtasks.Any(task =>
                task.Name.Equals(
                    name,
                    StringComparison.OrdinalIgnoreCase)))
            {
                await ShowErrorDialogue(
                    "A subtask with this name already exists at this level");

                return;
            }

            // Creating the new subtask under the target task
            TaskItem subtask = new()
            {
                Name = name,
                Description = description
            };

            ViewModel.AddSubtask(task, subtask);
        }

        // Edit Task
        private async void EditTaskButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (ViewModel.SelectedItem is not TaskItem task) return;

            // Prompting for updated task details
            var details = await PromptTaskDetails("Edit Task", task);

            if (!details.HasValue)
                return;

            var (name, description) = details.Value;

            if (task.Parent is not TaskItem parent) return;

            // Validating the new task details (local uniqueness)
            if (parent.Subtasks.Any(other =>
                other.Id != task.Id &&
                other.Name.Equals(
                    name,
                    StringComparison.OrdinalIgnoreCase)))
            {
                await ShowErrorDialogue(
                    "A task with this name already exists at this level.");

                return;
            }

            task.Name = name;
            task.Description = description;

            ViewModel.RefreshTasks();
        }

        // Delete Task
        private async void DeleteTaskButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (ViewModel.SelectedItem is not TaskItem task) return;

            bool confirmed = await PromptDeleteTask(task);
            if (!confirmed) return;

            ViewModel.DeleteTask(task);
        }

        // Toggle Task Action Visibility
        private void TasksListView_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            // Hide the action buttons for every visible task
            foreach (var taskEntry in ViewModel.FilteredTasks)
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

        private async void TaskCompletionCheckBox_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (ViewModel.SelectedItem is not TaskItem task) return;

            ViewModel.RefreshTasks();
        }

        public void ToggleTaskExpansionButton_Click(
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
            ViewModel.ToggleTaskExpansion(taskId);
        }



        // [[ DIALOGUES ]] //

        public async Task<(string name, string description)?>
            PromptTaskDetails(string title, TaskItem? task)
        {
            // Creating a dialogue box
            TextBox nameTextBox = new()
            {
                Header = "Name",
                PlaceholderText = "Enter a task name",
                Text = task?.Name ?? string.Empty
            };

            TextBox descriptionTextBox = new()
            {
                Header = "Description",
                PlaceholderText = "Enter a task description",
                Text = task?.Description ?? string.Empty,
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
                return null;

            string name = nameTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                await ShowErrorDialogue("Please enter a task name.");
                return null;
            }

            return (
                name,
                descriptionTextBox.Text.Trim());
        }

        public async Task<bool> PromptDeleteTask(TaskItem task)
        {
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
            bool confirmed = result == ContentDialogResult.Primary;

            return confirmed;
        }

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
    }
}
