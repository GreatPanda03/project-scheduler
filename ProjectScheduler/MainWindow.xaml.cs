
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ProjectScheduler.Pages;
using ProjectScheduler.Services;

namespace ProjectScheduler;

public sealed partial class MainWindow : Window
{
    public static MainWindow? Current { get; private set; }

    public MainWindow()
    {
        InitializeComponent();

        Current = this;

        RootNavigation.SelectedItem = HomeNavigationItem;
        ContentFrame.Navigate(typeof(HomePage));

        ChecklistAccessState.AccessChanged += UpdateNavigationAccess;

        UpdateNavigationAccess();
    }

    private void UpdateNavigationAccess()
    {
        bool unlocked = ChecklistAccessState.IsUnlocked;

        // Keep Home accessible at all times.
        foreach (var menuItem in RootNavigation.MenuItems)
        {
            if (menuItem is NavigationViewItem item)
            {
                string? pageName = item.Tag?.ToString();

                item.IsEnabled = pageName == "Home" || unlocked;
            }
        }

        // Apply the same rule to footer navigation items, if present.
        foreach (var menuItem in RootNavigation.FooterMenuItems)
        {
            if (menuItem is NavigationViewItem item)
            {
                item.IsEnabled = unlocked;
            }
        }
    }

    private void RootNavigation_SelectionChanged(
        NavigationView sender,
        NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItemContainer is not NavigationViewItem item)
            return;

        string? pageName = item.Tag?.ToString();

        // Safety check in case navigation is attempted while locked.
        if (pageName != "Home" && !ChecklistAccessState.IsUnlocked)
        {
            RootNavigation.SelectedItem = HomeNavigationItem;
            return;
        }

        NavigateToPage(pageName);
    }

    private void NavigateToPage(string? pageName)
    {
        switch (pageName)
        {
            case "Home":
                ContentFrame.Navigate(typeof(HomePage));
                break;

            case "Pre-work Checklist":
                ContentFrame.Navigate(typeof(PreWorkChecklistPage));
                break;

            default:
                ContentFrame.Navigate(
                    typeof(PlaceholderPage),
                    pageName
                );
                break;
        }
    }

    public void NavigateToSchedules()
    {
        if (!ChecklistAccessState.IsUnlocked)
            return;

        // Select the Schedules navigation item.
        foreach (var menuItem in RootNavigation.MenuItems)
        {
            if (menuItem is NavigationViewItem item &&
                item.Tag?.ToString() == "Schedules")
            {
                RootNavigation.SelectedItem = item;
                break;
            }
        }
    }
}
