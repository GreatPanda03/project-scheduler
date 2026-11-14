using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ProjectScheduler.Enums;
using ProjectScheduler.Models;
using ProjectScheduler.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using Windows.Storage;
using Windows.System;

namespace ProjectScheduler.Pages {
    public sealed partial class ResourcesPage : Page
    {
        private readonly ObservableCollection<ResourceItem> _resources = new();
        private readonly ObservableCollection<ResourceItem> _filteredResources = new();
        private readonly ResourceService _resourceService = new();

        // Used to check availability of website URLs
        private static readonly HttpClient _httpClient = new()
        {
            Timeout = TimeSpan.FromSeconds(10)
        };


        public ResourcesPage()
        {
            InitializeComponent();
            
            ResourcesListView.ItemsSource = _filteredResources;

            // Populate the Type ComboBox using the ResourceType enum
            ResourceTypeComboBox.ItemsSource = Enum.GetValues<ResourceType>();

            Loaded += ResourcesPage_Loaded;
        }


        // [[ LOADING RESOURCES ]] //

        private async void ResourcesPage_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                var savedResources = await _resourceService.LoadAsync();

                _resources.Clear();

                foreach (var resource in savedResources)
                {
                    _resources.Add(resource);
                }

                RefreshResourceList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Could not load resources: {ex.Message}"
                );

                await ShowErrorDialogue(
                    "Could not load the saved resources."
                );
            }
        }


        // [[ REFRESHING & SEARCHING RESOURCES ]] //

        private void RefreshResourceList()
        {
            string searchText = SearchTextBox?.Text ?? string.Empty;

            var filtered = _resources
                .Where(resource =>
                    resource.Name.Contains(
                        searchText,
                        System.StringComparison.OrdinalIgnoreCase))
                .OrderBy(resource => resource.Name)
                .ToList();

            _filteredResources.Clear();

            foreach (var resource in filtered)
            {
                _filteredResources.Add(resource);
            }
        }

        private void SearchTextBox_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            RefreshResourceList();
        }


        // [[ ADDING RESOURCES ]] //

        private async void AddResourceButton_Click(
            object sender,
            RoutedEventArgs e)
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
                TextWrapping = TextWrapping.Wrap
            };

            var typeComboBox = new ComboBox
            {
                Header = "Resource Type",
                ItemsSource = Enum.GetValues<ResourceType>(),
                SelectedIndex = 0,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            var directoryTextBox = new TextBox
            {
                Header = "Directory",
                PlaceholderText = "Enter a file path or URL"
            };

            var dialogueContent = new StackPanel
            {
                Spacing = 12
            };

            dialogueContent.Children.Add(nameTextBox);
            dialogueContent.Children.Add(descriptionTextBox);
            dialogueContent.Children.Add(typeComboBox);
            dialogueContent.Children.Add(directoryTextBox);

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
                Directory = directoryTextBox.Text.Trim()
            };

            _resources.Add(newResource);

            RefreshResourceList();

            // Select the newly added resource
            ResourcesListView.SelectedItem =
                _filteredResources.FirstOrDefault(
                    resource => resource.Id == newResource.Id);

            // Automatically save the updated collection.
            await SaveResourcesAsync();
        }


        // [[ DISPLAYING SELECTED RESOURCE ]] //

        private void ResourcesListView_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (ResourcesListView.SelectedItem is ResourceItem resource)
            {
                DisplayResourceDetails(resource);
            }
            else 
            {
                ShowResourcePlaceholder();
            }
        }

        private void DisplayResourceDetails(ResourceItem resource)
        {
            ResourceNameTextBox.Text = resource.Name;
            ResourceDescriptionTextBox.Text = resource.Description;
            ResourceTypeComboBox.SelectedItem = resource.Type;
            ResourceDirectoryTextBox.Text = resource.Directory;
            
            ResourcePlaceholderPanel.Visibility = Visibility.Collapsed;
            ResourceDetailsPanel.Visibility = Visibility.Visible;
        }

        private void ShowResourcePlaceholder()
        {
            ResourceDetailsPanel.Visibility = Visibility.Collapsed;
            ResourcePlaceholderPanel.Visibility = Visibility.Visible;
        }


        // [[ OPENING RESOURCES ]] //

        private async void OpenResourceButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (ResourcesListView.SelectedItem is not ResourceItem)
            {
                return;
            }

            // Read values as they currently are in the details panel
            //   so that the user can test availability without saving
            string directory = ResourceDirectoryTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(directory))
            {
                await ShowErrorDialogue(
                    "The resource couldn't be found. " +
                    "Please check that you have entered a directory."
                );

                return;
            }

            if (ResourceTypeComboBox.SelectedItem is not ResourceType selectedType)
            {
                await ShowErrorDialogue(
                    "Please select a valid resource type."
                );

                return;
            }

            try
            {
                switch (selectedType)
                {
                    case ResourceType.Document:
                        await OpenDocumentAsync(directory);
                        break;

                    case ResourceType.Application:
                        await OpenApplication(directory);
                        break;

                    case ResourceType.Website:
                        await OpenWebsiteAsync(directory);
                        break;

                    default:
                        await ShowErrorDialogue(
                            $"The selected resource type " +
                            "\"{selectedType.ToString()}\" is no supported."
                        );
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"Error opening resources: {ex}"
                );

                await ShowErrorDialogue(
                    "The resource coudln't be found or opened. " +
                    "Please check its directory and try again."
                );
            }
        }

        private async Task OpenDocumentAsync(string directory)
        {
            if (!File.Exists(directory))
            {
                await ShowErrorDialogue(
                    "The document couldn't be found. Please check its directory."
                );

                return;
            }

            StorageFile file = await StorageFile.GetFileFromPathAsync(directory);

            bool launched = await Launcher.LaunchFileAsync(file);

            if (!launched)
            {
                await ShowErrorDialogue(
                    "The document couldn't be opened. Please check its directory " +
                    "and ensure Windows has an app associated with this file type."
                );
            }
        }

        private async Task OpenApplication(string directory)
        {
            if (!File.Exists(directory))
            {
                await ShowErrorDialogue(
                    "The application couldn't be found. " +
                    "Please check the application is installed at the location."
                );

                return;
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = directory,
                UseShellExecute = true
            });
        }

        private async Task OpenWebsiteAsync(string directory)
        {
            // Check that the directory is a valid web address
            if (!Uri.TryCreate(
                    directory,
                    UriKind.Absolute,
                    out Uri? uri) ||
                (uri.Scheme != Uri.UriSchemeHttp &&
                 uri.Scheme != Uri.UriSchemeHttps))
            {
                await ShowErrorDialogue(
                    "The web address is invalid. " +
                    "Please ensure its directory begins with http:// or https://."
                );

                return;
            }

            // Check whether the website can be reached
            try
            {
                using var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    uri
                );

                request.Headers.UserAgent.ParseAdd(
                    "ProjectScheduler/1.0"
                );

                using HttpResponseMessage response =
                    await _httpClient.SendAsync(
                        request,
                        HttpCompletionOption.ResponseHeadersRead
                    );

                if (response.StatusCode == HttpStatusCode.NotFound ||
                    response.StatusCode == HttpStatusCode.Gone)
                {
                    await ShowErrorDialogue(
                        "The web resource couldn't be found. " +
                        "Please check the resource exists at the directory."
                    );

                    return;
                }
            }
            catch (HttpRequestException)
            {
                await ShowErrorDialogue(
                    "The website couldn't be reached. " +
                    "Please check your Internet connection and whether " +
                    "the resource is still up."
                );

                return;
            }
            catch (TaskCanceledException)
            {
                await ShowErrorDialogue(
                    "The website took too long to respond. " +
                    "Please check your Internet connection and whether " +
                    "the resource is still up."
                );
            }

            // Launch the user's default browser with the URI
            bool launched = await Launcher.LaunchUriAsync(uri);

            if (!launched)
            {
                await ShowErrorDialogue(
                    "The website couldn't be opened. " +
                    "Please ensure the directory is valid and ensure a " +
                    "default browser is installed."
                );
            }
        }


        // [[ SAVING RESOURCE CHANGES ]] //

        private async void SaveResourceButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (ResourcesListView.SelectedItem is not ResourceItem resource)
            {
                return;
            }

            // Validation
            string name = ResourceNameTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                await ShowErrorDialogue(
                    "A resource must have a name. Please enter a name before saving."
                );

                return;
            }

            if (ResourceTypeComboBox.SelectedItem is not ResourceType selectedType ||
                !Enum.IsDefined(typeof(ResourceType), selectedType))
            {
                await ShowErrorDialogue(
                    "Please select a valid resource type."
                );

                return;
            }

            // Updating the selected resource
            resource.Name = name;
            resource.Description = ResourceDescriptionTextBox.Text.Trim();
            resource.Type = selectedType;
            resource.Directory = ResourceDirectoryTextBox.Text.Trim();

            // Refresh the list to apply alphabetical sorting
            RefreshResourceList();

            // Reselect the updated resource
            ResourcesListView.SelectedItem =
                _filteredResources.FirstOrDefault(
                    item => item.Id == resource.Id);

            // Automatically save the updated collection
            await SaveResourcesAsync();
        }


        // [[ DELETING RESOURCES ]] //

        private async void DeleteResourceButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (ResourcesListView.SelectedItem is not ResourceItem resource)
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

            _resources.Remove(resource);

            // Clear the current selection and refresh the list
            ResourcesListView.SelectedItem = null;

            ShowResourcePlaceholder();

            RefreshResourceList();

            await SaveResourcesAsync();
        }



        // [[ JSON PERSISTENCE ]] //

        private async Task SaveResourcesAsync()
        {
            try
            {
                await _resourceService.SaveAsync(_resources);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Could not save resources: {ex.Message}"
                );

                await ShowErrorDialogue(
                    "The resource could not be saved. Please try again."
                );
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

