using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ProjectScheduler.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System;
using ResourceType = ProjectScheduler.Enums.ResourceType;

namespace ProjectScheduler.Services
{
    public class ResourceOpener
    {
        // todo: uncomment error dialogues here

        // Used to check availability of website URLs
        private static readonly HttpClient _httpClient = new()
        {
            Timeout = TimeSpan.FromSeconds(10)
        };


        public async void OpenResource(ResourceItem resource)
        {
            if (resource is not ResourceItem)
            {
                return;
            }

            string location = resource.Location;

            if (string.IsNullOrWhiteSpace(location))
            {
                await ShowErrorDialogue(
                    "The resource couldn't be found. " +
                    "Please check that you have entered a location.");

                return;
            }

            if (!(resource.Type is ResourceType type &&
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


        // [[ ERROR DIALOGUE ]] //
        private static async Task ShowErrorDialogue(string message)
        {
            var dialogue = new ContentDialog
            {
                Title = "Resource Error",
                Content = message,
                CloseButtonText = "OK"
                // XamlRoot = XamlRoot
            };

            await dialogue.ShowAsync();
        }
    }
}
