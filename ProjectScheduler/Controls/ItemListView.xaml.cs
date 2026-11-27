using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections;

namespace ProjectScheduler.Controls
{
    public sealed partial class ItemListView : UserControl
    {
        public ItemListView()
        {
            InitializeComponent();
        }
        
        // Title
        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                nameof(Title),
                typeof(string),
                typeof(ItemListView),
                new PropertyMetadata("Items"));

        // Search placeholder
        public string SearchPlaceholder
        {
            get => (string)GetValue(SearchPlaceholderProperty);
            set => SetValue(SearchPlaceholderProperty, value);
        }

        public static readonly DependencyProperty SearchPlaceholderProperty =
            DependencyProperty.Register(
                nameof(SearchPlaceholder),
                typeof(string),
                typeof(ItemListView),
                new PropertyMetadata("Search..."));

        // Add button text
        public string AddButtonText
        {
            get => (string)GetValue(AddButtonTextProperty);
            set => SetValue(AddButtonTextProperty, value);
        }

        public static readonly DependencyProperty AddButtonTextProperty =
            DependencyProperty.Register(
                nameof(AddButtonText),
                typeof(string),
                typeof(ItemListView),
                new PropertyMetadata("Add"));

        // Search text
        public string SearchText
        {
            get => (string)GetValue(SearchTextProperty);
            set => SetValue(SearchTextProperty, value);
        }

        public static readonly DependencyProperty SearchTextProperty =
            DependencyProperty.Register(
                nameof(SearchText),
                typeof(string),
                typeof(ItemListView),
                new PropertyMetadata(string.Empty));

        // Items source
        public IEnumerable? ItemsSource
        {
            get => (IEnumerable?)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                nameof(ItemsSource),
                typeof(IEnumerable),
                typeof(ItemListView),
                new PropertyMetadata(null));

        // Selected item
        public object? SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(
                nameof(SelectedItem),
                typeof(object),
                typeof(ItemListView),
                new PropertyMetadata(null));

        // Item template
        public DataTemplate? ItemTemplate
        {
            get => (DataTemplate?)GetValue(ItemTemplateProperty);
            set => SetValue(ItemTemplateProperty, value);
        }

        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register(
                nameof(ItemTemplate),
                typeof(DataTemplate),
                typeof(ItemListView),
                new PropertyMetadata(null));

        // Add button event
        public event EventHandler? AddRequested;

        private void AddButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            AddRequested?.Invoke(this, EventArgs.Empty);
        }


        // Selection changed event
        public event SelectionChangedEventHandler? SelectionChanged;

        private void ItemsListView_SelectionChanged(
            object sender, 
            SelectionChangedEventArgs e)
        {
            SelectionChanged?.Invoke(this, e);
        }
    }
}
