using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ProjectScheduler.Models;
using ProjectScheduler.ViewModels;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectScheduler.Pages
{
    public sealed partial class SubslotsPage : Page
    {
        private SubslotViewModel ViewModel { get; }

        public SubslotsPage()
        {
            InitializeComponent();

            ViewModel = new SubslotViewModel();

            DataContext = ViewModel;

            Loaded += SubslotsPage_Loaded;
        }


        // [[ LOADING SUBSLOTS ]] //

        private async void SubslotsPage_Loaded(
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
                    $"Could not load subslots: {ex.Message}");

                await ShowErrorDialogue(
                    "Could not load the saved subslots");
            }
        }


        // [[ CREATING SUBSLOTS ]] //

        private async void AddSubslotButton_Click(
            object sender,
            EventArgs e)
        {
            // Creating the dialogue box
            TextBox nameTextBox = new()
            {
                Header = "Name",
                PlaceholderText = "Enter a subslot name"
            };

            TextBox descriptionTextBox = new()
            {
                Header = "Description",
                PlaceholderText = "Enter a description",
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                MinHeight = 80
            };

            TextBlock rulesHeader = new()
            {
                Text = "RULES",
                FontSize = 12,
                FontWeight = FontWeights.SemiBold
            };

            CheckBox lockResourcesCheckBox = new()
            {
                Content = "Lock Resources"
            };

            StackPanel dialogueContent = new()
            {
                Spacing = 12
            };

            dialogueContent.Children.Add(nameTextBox);
            dialogueContent.Children.Add(descriptionTextBox);
            dialogueContent.Children.Add(rulesHeader);
            dialogueContent.Children.Add(lockResourcesCheckBox);

            ContentDialog dialogue = new()
            {
                Title = "Add Subslot",
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

            // Validate the provided Subslot details
            string name = nameTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                await ShowErrorDialogue(
                    "Please enter a name for the subslot.");

                return;
            }

            // Create and display the new Subslot if all is good
            var newSubslot = new SubslotItem
            {
                Name = name,
                Description = descriptionTextBox.Text.Trim(),
                LockResources = lockResourcesCheckBox.IsChecked ?? false
            };

            ViewModel.AddSubslot(newSubslot);

            await SaveSubslotsAsync();
        }


        // [[ REFRESHING & SEARCHING SUBSLOTS ]] //
        private void RefreshSubslotList()
        {
            if (ViewModel.SelectedItem is SubslotItem subslot)
            {
                DisplaySubslotDetails(subslot);
            }
            else
            {
                ShowSubslotPlaceholder();
            }
        }


        // [[ DISPLAYING SELECTED SUBSLOT ]] //

        private void SubslotsListView_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            SubslotItem? selectedSubslot =
                e.AddedItems.OfType<SubslotItem>().FirstOrDefault();

            ViewModel.SelectedItem = selectedSubslot;

            if (selectedSubslot != null)
            {
                DisplaySubslotDetails(selectedSubslot);
            }
            else
            {
                ShowSubslotPlaceholder();
            }
        }

        private void DisplaySubslotDetails(SubslotItem subslot)
        {
            SubslotNameTextBox.Text = subslot.Name;
            
            SubslotDescriptionTextBox.Text =
                subslot.Description;
            
            SubslotLockResourcesCheckBox.IsChecked =
                subslot.LockResources;

            SubslotPlaceholderPanel.Visibility =
                Visibility.Collapsed;

            SubslotDetailsPanel.Visibility =
                Visibility.Visible;
        }

        private void ShowSubslotPlaceholder()
        {
            SubslotDetailsPanel.Visibility =
                Visibility.Collapsed;

            SubslotPlaceholderPanel.Visibility =
                Visibility.Visible;
        }


        // [[ SAVING SUBSLOT CHANGES ]] //

        private async void SaveSubslotButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (ViewModel.SelectedItem is not SubslotItem subslot)
            {
                return;
            }

            // Validation
            string name = SubslotNameTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                await ShowErrorDialogue(
                    "A subslot must have a name. Please enter a name before saving.");

                return;
            }

            // Updating the selected subslot
            subslot.Name = name;
            
            subslot.Description =
                SubslotDescriptionTextBox.Text.Trim();
            
            subslot.LockResources =
                SubslotLockResourcesCheckBox.IsChecked ?? false;

            // Refresh the list to apply alphabetical sorting
            ViewModel.RefreshItems();

            // Reselect the updated subslot
            ViewModel.SelectedItem =
                ViewModel.FilteredItems.FirstOrDefault(
                    item => item.Id == subslot.Id);

            // Automatically save the updated collection
            await SaveSubslotsAsync();
        }


        // [[ DELETING SUBSLOTS ]] //

        private async void DeleteSubslotButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (ViewModel.SelectedItem is not SubslotItem subslot)
            {
                return;
            }

            // Prompt confirmation
            var dialogue = new ContentDialog
            {
                Title = "Delete Subslot",
                Content = $"Are you sure you want to delete \"{subslot.Name}\"? This action cannot be undone!",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = XamlRoot
            };

            var result = await dialogue.ShowAsync();

            // Remove the subslot if confirmed
            if (result != ContentDialogResult.Primary)
            {
                return;
            }

            // Clear the current selection and refresh the list
            ViewModel.DeleteSubslot(subslot);

            await SaveSubslotsAsync();
        }


        // [[ JSON PERSISTENCE ]] //

        private async Task SaveSubslotsAsync()
        {
            try
            {
                await ViewModel.SaveAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"Could not save subslots: {ex.Message}");

                await ShowErrorDialogue(
                    "The subslot could not be saved. Please try again.");
            }
        }


        // [[ ERROR DIALOGUE ]] //

        private async Task ShowErrorDialogue(string message)
        {
            var dialogue = new ContentDialog
            {
                Title = "Subslot Error",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = XamlRoot
            };

            await dialogue.ShowAsync();
        }
    }
}
