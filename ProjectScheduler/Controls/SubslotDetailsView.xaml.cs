using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ProjectScheduler.Models;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ProjectScheduler.Controls
{
    public sealed partial class SubslotDetailsView : UserControl
    {
        public SubslotDetailsView()
        {
            InitializeComponent();
        }

        // Selected Subslot
        public SubslotItem? Subslot
        {
            get => (SubslotItem?)GetValue(SubslotProperty);
            set => SetValue(SubslotProperty, value);
        }

        public static readonly DependencyProperty SubslotProperty =
            DependencyProperty.Register(
                nameof(Subslot),
                typeof(SubslotItem),
                typeof(SubslotDetailsView),
                new PropertyMetadata(null, OnSubslotChanged));

        // Update the displayed subslot
        private static void OnSubslotChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var control = (SubslotDetailsView)d;

            control.DetailsPanel.DataContext = e.NewValue;

            bool hasSubslot = e.NewValue is SubslotItem;

            control.PlaceholderPanel.Visibility =
                hasSubslot ? Visibility.Collapsed : Visibility.Visible;

            control.DetailsPanel.Visibility =
                hasSubslot ? Visibility.Visible : Visibility.Collapsed;
        }

        // Save event
        public event EventHandler? SaveRequested;

        private void SaveButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            SaveRequested?.Invoke(this, EventArgs.Empty);
        }

        // Delete event
        public event EventHandler? DeleteRequested;

        private void DeleteButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            DeleteRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
