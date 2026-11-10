
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ProjectScheduler.Pages;

namespace ProjectScheduler;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // Select Home when the application starts.
        RootNavigation.SelectedItem = HomeNavigationItem;

        // Display the Home page.
        ContentFrame.Navigate(typeof(HomePage));
    }

    private void RootNavigation_SelectionChanged(
        NavigationView sender,
        NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItemContainer is NavigationViewItem item)
        {
            string pageName = item.Tag.ToString();

            // Navigate to the appropriate page.
            if (pageName == "Home")
            {
                ContentFrame.Navigate(typeof(HomePage));
            }
            else
            {
                // Other sections still use the placeholder page.
                ContentFrame.Navigate(
                    typeof(PlaceholderPage),
                    pageName
                );
            }
        }
    }
}
