using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ProjectScheduler.Models;
using System.Threading.Tasks;
using System.Diagnostics;
using ProjectScheduler.ViewModels;
using System.Reflection.Metadata.Ecma335;

namespace ProjectScheduler.Pages
{
    public sealed partial class ProjectsPage : Page
    {
        private readonly ProjectViewModel _projectViewModel = new();
        private readonly ResourceViewModel _resourceViewModel = new();
        private readonly ObservableCollection<ResourceItem> _projectResources = new();
        private bool _isLoadingProjects;


        // [[ CONSTRUCTOR ]] //

        public ProjectsPage()
        {
            InitializeComponent();

            DataContext = _projectViewModel;

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

                await _projectViewModel.LoadAsync();
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
                await _projectViewModel.SaveAsync();
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
                await _resourceViewModel.LoadAsync();

                if (_projectViewModel.SelectedItem is ProjectItem project)
                {
                    _resourceViewModel.RefreshItems();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"Could not load resources: {ex.Message}");   
            }
        }


        // [[ CREATING PROJECTS ]] //

        private async void OnAddProjectRequested(
            object sender,
            EventArgs e)
        {
            var details = await PromptProjectDetails();

            if (!details.HasValue) return;

            (string name, string description, float priority) = 
                details.Value;

            // Create the project and refresh the list
            ProjectItem newProject = new()
            {
                Name = name,
                Description = description,
                Priority = priority
            };

            _projectViewModel.AddProject(newProject);

            await SaveProjectsAsync();
        }


        // [[ SAVING PROJECTS ]] //

        private async void OnSaveProjectRequested(
            object sender,
            EventArgs e)
        {
            if (_projectViewModel.SelectedItem is not ProjectItem project)
                return;

            string name = project.Name.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                await ShowErrorDialogue(
                    "A project must have a name.");
                return;
            }

            float priority = (float)project.Priority;

            if (float.IsNaN(priority) || priority < 0)
            {
                await ShowErrorDialogue(
                    "A project must have a positive priority");
                return;
            }

            project.Name = name;
            project.Description = project.Description.Trim();
            project.Priority = priority;
            project.Notes = project.Notes.Trim();

            _projectViewModel.SelectedItem =
                _projectViewModel.Items.FirstOrDefault(
                    item => item.Id == project.Id);

            await SaveProjectsAsync();
        }


        // [[ DELETING PROJECTS ]] //
        private async void OnDeleteProjectRequested(
            object sender,
            EventArgs e)
        {
            if (_projectViewModel.SelectedItem is not ProjectItem project)
                return;

            bool confirmed = await PromptDeleteProject(project);
            if (!confirmed) return;

            _projectViewModel.RemoveItem(_projectViewModel.SelectedItem);

            // Refresh the list to apply alphabetical sorting
            _projectViewModel.RefreshItems();
            
            // Automatically save the updated collection
            await SaveProjectsAsync();
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


        // [[ DIALOGUES ]] //

        private async Task<(string name, string description, float priority)?>
            PromptProjectDetails()
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

            // Prompt for the results
            var result = await dialogue.ShowAsync();

            if (result != ContentDialogResult.Primary)
                return null;

            // Validate the input
            string name = nameTextBox.Text.Trim();
            string description = descriptionTextBox.Text.Trim();
            float priority = (float)priorityNumberBox.Value;

            if (string.IsNullOrWhiteSpace(name))
            {
                await ShowErrorDialogue(
                    "Please enter a name for the project.");

                return null;
            }

            if (float.IsNaN(priority) || priority < 0)
            {
                await ShowErrorDialogue(
                    "Please enter a numerical priority for the project.");

                return null;
            }

            return (name, description, priority);
        }

        private async Task<bool>
            PromptDeleteProject(ProjectItem project)
        {
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
    }
}
