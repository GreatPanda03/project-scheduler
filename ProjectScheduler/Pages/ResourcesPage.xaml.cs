using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ProjectScheduler.Enums;
using ProjectScheduler.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using ProjectScheduler.ViewModels;

namespace ProjectScheduler.Pages {
    public sealed partial class ResourcesPage : Page
    {
        private ResourceViewModel ViewModel { get; }

        public ResourcesPage()
        {
            InitializeComponent();

            ViewModel = new ResourceViewModel();

            DataContext = ViewModel;

            Loaded += ResourcesPage_Loaded;
        }


        // [[ LOADING RESOURCES ]] //

        private async void ResourcesPage_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                await ViewModel.LoadAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"Could not load resources: {ex.Message}");

                await ShowErrorDialogue(
                    "Could not load the saved resources.");
            }
        }


        // [[ ADDING RESOURCES ]] //

        private async void AddResourceButton_Click(
            object sender,
            EventArgs e)
        {

            // Building the dialogue box to enter Resource details
            var nameTextBox = new TextBox
            {
                Header = "Resource Name",
                PlaceholderText = "Enter a name"
            };

            var descriptionTextBox = new TextBox
            { 
                Header = "Description",
                PlaceholderText = "Enter a description",
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                MinHeight = 80
            };

            var typeComboBox = new ComboBox
            {
                Header = "Resource Type",
                ItemsSource = Enum.GetValues<ResourceType>(),
                SelectedIndex = 0,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            var locationTextBox = new TextBox
            {
                Header = "Location",
                PlaceholderText = "Enter a file path or URL"
            };

            var dialogueContent = new StackPanel
            {
                Spacing = 12
            };

            dialogueContent.Children.Add(nameTextBox);
            dialogueContent.Children.Add(descriptionTextBox);
            dialogueContent.Children.Add(typeComboBox);
            dialogueContent.Children.Add(locationTextBox);

            var dialogue = new ContentDialog
            {
                Title = "Add Resource",
                Content = dialogueContent,
                PrimaryButtonText = "Add",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = XamlRoot
            };

            // Display the dialogue box and await input
            var result = await dialogue.ShowAsync();

            if (result != ContentDialogResult.Primary)
            {
                return;
            }

            // Validate the provided Resource details
            string name = nameTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                await ShowErrorDialogue(
                    "Please enter a name for the resource."
                );

                return;
            }

            // Validate the resource type
            if (typeComboBox.SelectedItem is not ResourceType selectedType ||
                !Enum.IsDefined(typeof(ResourceType), selectedType))
            {
                await ShowErrorDialogue(
                   "Please select a valid resource type."
                );

                return;
            }

            // Create and display the new Resource if all is good
            var newResource = new ResourceItem
            { 
                Name = name,
                Description = descriptionTextBox.Text.Trim(),
                Type = (ResourceType) typeComboBox.SelectedItem!,
                Location = locationTextBox.Text.Trim()
            };

            ViewModel.AddResource(newResource);

            await SaveResourcesAsync();
        }


        // [[ OPENING RESOURCES ]] //

        private async void OpenResourceButton_Click(
            object sender,
            EventArgs e)
        {
            if (ViewModel.SelectedItem is not ResourceItem)
            {
                return;
            }

            ViewModel.OpenResource(ViewModel.SelectedItem);
        }


        // [[ SAVING RESOURCE CHANGES ]] //

        private async void SaveResourceButton_Click(
            object sender,
            EventArgs e)
        {
            if (ViewModel.SelectedItem is not ResourceItem resource)
            {
                return;
            }

            // Validation
            string name = ViewModel.SelectedItem.Name;

            if (string.IsNullOrWhiteSpace(name))
            {
                await ShowErrorDialogue(
                    "A resource must have a name. Please enter a name before saving.");

                return;
            }

            if (!(ViewModel.SelectedItem.Type is ResourceType type &&
                Enum.IsDefined(typeof(ResourceType), type)))
            {
                await ShowErrorDialogue(
                    "Please select a valid resource type.");

                return;
            }

            // Updating the selected resource
            resource.Name = name;
            resource.Description = ViewModel.SelectedItem.Description.Trim();
            resource.Type = ViewModel.SelectedItem.Type;
            resource.Location = ViewModel.SelectedItem.Location.Trim();

            // Refresh the list to apply alphabetical sorting
            ViewModel.RefreshItems();

            // Reselect the updated resource
            ViewModel.SelectedItem =
                ViewModel.FilteredItems.FirstOrDefault(
                    item => item.Id == resource.Id);

            // Automatically save the updated collection
            await SaveResourcesAsync();
        }


        // [[ DELETING RESOURCES ]] //

        private async void DeleteResourceButton_Click(
            object sender,
            EventArgs e)
        {
            if (ViewModel.SelectedItem is not ResourceItem resource)
            {
                return;
            }

            // Prompt confirmation
            var dialogue = new ContentDialog
            {
                Title = "Delete Resource",
                Content = $"Are you sure you want to delete \"{resource.Name}\"? This action cannot be undone!",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = XamlRoot
            };

            var result = await dialogue.ShowAsync();

            // Remove the resource if confirmed
            if (result != ContentDialogResult.Primary)
            {
                return;
            }

            ViewModel.RemoveItem(resource);

            // Clear the current selection and refresh the list
            ViewModel.SelectedItem = null;

            ViewModel.RefreshItems();

            await SaveResourcesAsync();
        }



        // [[ JSON PERSISTENCE ]] //

        private async Task SaveResourcesAsync()
        {
            try
            {
                await ViewModel.SaveAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"Could not save resources: {ex.Message}");

                await ShowErrorDialogue(
                    "The resource could not be saved. Please try again.");
            }
        }


        // [[ ERROR DIALOGUE ]] //

        private async Task ShowErrorDialogue(string message)
        {
            var dialogue = new ContentDialog
            { 
                Title = "Resource Error",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = XamlRoot
            };

            await dialogue.ShowAsync();
        }
    }
}

