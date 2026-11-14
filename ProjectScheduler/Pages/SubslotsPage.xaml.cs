using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ProjectScheduler.Models;
using ProjectScheduler.Services;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectScheduler.Pages
{
    public sealed partial class SubslotsPage : Page
    {
        private readonly ObservableCollection<SubslotItem> _subslots = new();
        private readonly ObservableCollection<SubslotItem> _filteredSubslots = new();
        private readonly SubslotService _subslotService = new();

        public SubslotsPage()
        {
            InitializeComponent();

            SubslotsListView.ItemsSource = _filteredSubslots;

            Loaded += SubslotsPage_Loaded;
        }


        // [[ LOADING SUBSLOTS ]] //

        private async void SubslotsPage_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            await LoadSubslotsAsync();
        }

        private async Task LoadSubslotsAsync()
        {
            try
            {
                var savedSubslots = await _subslotService.LoadAsync();

                _subslots.Clear();

                foreach (var subslot in savedSubslots)
                {
                    _subslots.Add(subslot);
                }

                RefreshSubslotList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"Could not load subslots: {ex.Message}"
                );

                await ShowErrorDialogue(
                    "Could not load the saved subslots."
                );
            }
        }


        // [[ CREATING SUBSLOTS ]] //

        private async void AddSubslotButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            // Creating the dialogue box
            TextBox nameTextBox = new()
            {
                Header = "Subslot Name",
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
                    "Please enter a name for the subslot."
                );

                return;
            }

            // Create and display the new Subslot if all is good
            var newSubslot = new SubslotItem
            {
                Name = name,
                Description = descriptionTextBox.Text.Trim(),
                LockResources = lockResourcesCheckBox.IsChecked ?? false
            };

            _subslots.Add(newSubslot);

            RefreshSubslotList();

            // Select the newly added Subslot
            SubslotsListView.SelectedItem =
                _filteredSubslots.FirstOrDefault(
                    subslot => subslot.Id == newSubslot.Id);

            // Automatically save the updated collection.
            await SaveSubslotsAsync();
        }


        // [[ REFRESHING & SEARCHING SUBSLOTS ]] //
        private void RefreshSubslotList()
        {
            string searchText = SearchTextBox?.Text ?? string.Empty;

            var filtered = _subslots
                .Where(subslot =>
                    subslot.Name.Contains(
                        searchText,
                        StringComparison.OrdinalIgnoreCase))
                .OrderBy(subslot => subslot.Name)
                .ToList();

            _filteredSubslots.Clear();

            foreach (var subslot in filtered)
            {
                _filteredSubslots.Add(subslot);
            }
        }

        private void SearchTextBox_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            RefreshSubslotList();
        }


        // [[ DISPLAYING SELECTED SUBSLOT ]] //

        private void SubslotsListView_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (SubslotsListView.SelectedItem is SubslotItem subslot)
            {
                DisplaySubslotDetails(subslot);
            }
            else
            {
                ShowSubslotPlaceholder();
            }
        }

        private void DisplaySubslotDetails(SubslotItem subslot)
        {
            SubslotNameTextBox.Text = subslot.Name;
            SubslotDescriptionTextBox.Text = subslot.Description;
            SubslotLockResourcesCheckBox.IsChecked = subslot.LockResources;

            SubslotPlaceholderPanel.Visibility = Visibility.Collapsed;
            SubslotDetailsPanel.Visibility = Visibility.Visible;
        }

        private void ShowSubslotPlaceholder()
        {
            SubslotDetailsPanel.Visibility = Visibility.Collapsed;
            SubslotPlaceholderPanel.Visibility = Visibility.Visible;
        }


        // [[ SAVING SUBSLOT CHANGES ]] //

        private async void SaveSubslotButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (SubslotsListView.SelectedItem is not SubslotItem subslot)
            {
                return;
            }

            // Validation
            string name = SubslotNameTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                await ShowErrorDialogue(
                    "A subslot must have a name. Please enter a name before saving."
                );

                return;
            }

            // Updating the selected subslot
            subslot.Name = name;
            subslot.Description = SubslotDescriptionTextBox.Text.Trim();
            subslot.LockResources = SubslotLockResourcesCheckBox.IsChecked ?? false;

            // Refresh the list to apply alphabetical sorting
            RefreshSubslotList();

            // Reselect the updated subslot
            SubslotsListView.SelectedItem =
                _filteredSubslots.FirstOrDefault(
                    item => item.Id == subslot.Id);

            // Automatically save the updated collection
            await SaveSubslotsAsync();
        }


        // [[ DELETING SUBSLOTS ]] //

        private async void DeleteSubslotButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (SubslotsListView.SelectedItem is not SubslotItem subslot)
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

            _subslots.Remove(subslot);

            // Clear the current selection and refresh the list
            SubslotsListView.SelectedItem = null;

            ShowSubslotPlaceholder();

            RefreshSubslotList();

            await SaveSubslotsAsync();
        }



        // [[ JSON PERSISTENCE ]] //

        private async Task SaveSubslotsAsync()
        {
            try
            {
                await _subslotService.SaveAsync(_subslots);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"Could not save subslots: {ex.Message}"
                );

                await ShowErrorDialogue(
                    "The subslot could not be saved. Please try again."
                );
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
