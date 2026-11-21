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
using ProjectScheduler.ViewModels;

namespace ProjectScheduler.Pages {
    public sealed partial class ResourcesPage : Page
    {
        private ResourceViewModel ViewModel { get; }

        // Used to check availability of website URLs
        private static readonly HttpClient _httpClient = new()
        {
            Timeout = TimeSpan.FromSeconds(10)
        };


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

            // Select the newly added resource
            ViewModel.SelectedItem =
                ViewModel.FilteredItems.FirstOrDefault(
                    resource => resource.Id == newResource.Id);

            // Automatically save the updated collection.
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

            string location = ViewModel.SelectedItem.Location;

            if (string.IsNullOrWhiteSpace(location))
            {
                await ShowErrorDialogue(
                    "The resource couldn't be found. " +
                    "Please check that you have entered a location.");

                return;
            }

            if (!(ViewModel.SelectedItem.Type is ResourceType type &&
                Enum.IsDefined(typeof(ResourceType), type)))
            {
                await ShowErrorDialogue(
                    "Please select a valid resource type.");

                return;
            }

            try
            {
                switch (type)
                {
                    case ResourceType.Document:
                        await OpenDocumentAsync(location);
                        break;

                    case ResourceType.Application:
                        await OpenApplication(location);
                        break;

                    case ResourceType.Website:
                        await OpenWebsiteAsync(location);
                        break;

                    default:
                        await ShowErrorDialogue(
                            $"The selected resource type " +
                            "\"{selectedType.ToString()}\" is no supported.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"Error opening resources: {ex}");

                await ShowErrorDialogue(
                    "The resource coudln't be found or opened. " +
                    "Please check its location and try again.");
            }
        }

        private async Task OpenDocumentAsync(string location)
        {
            if (!File.Exists(location))
            {
                await ShowErrorDialogue(
                    "The document couldn't be found. Please check its location.");

                return;
            }

            StorageFile file = await StorageFile.GetFileFromPathAsync(location);

            bool launched = await Launcher.LaunchFileAsync(file);

            if (!launched)
            {
                await ShowErrorDialogue(
                    "The document couldn't be opened. Please check its location " +
                    "and ensure Windows has an app associated with this file type.");
            }
        }

        private async Task OpenApplication(string location)
        {
            if (!File.Exists(location))
            {
                await ShowErrorDialogue(
                    "The application couldn't be found. " +
                    "Please check the application is installed at the location."
                );

                return;
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = location,
                UseShellExecute = true
            });
        }

        private async Task OpenWebsiteAsync(string location)
        {
            // Check that the location is a valid web address
            if (!Uri.TryCreate(
                    location,
                    UriKind.Absolute,
                    out Uri? uri) ||
                (uri.Scheme != Uri.UriSchemeHttp &&
                 uri.Scheme != Uri.UriSchemeHttps))
            {
                await ShowErrorDialogue(
                    "The web address is invalid. " +
                    "Please ensure its location begins with http:// or https://."
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
                        "Please check the resource exists at the location."
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
                    "Please ensure the location is valid and ensure a " +
                    "default browser is installed."
                );
            }
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

