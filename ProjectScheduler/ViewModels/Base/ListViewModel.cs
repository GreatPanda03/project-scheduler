using CommunityToolkit.Mvvm.ComponentModel;
using ProjectScheduler.Interfaces;
using System.Collections.ObjectModel;
using System.Linq;

namespace ProjectScheduler.ViewModels.Base
{
    public abstract partial class ListViewModel<T> : ObservableObject
        where T : class, IListItem
    {
        public ObservableCollection<T> Items { get; } = new();

        public ObservableCollection<T> FilteredItems { get; } = new();

        [ObservableProperty]
        private T? selectedItem;

        [ObservableProperty]
        private string searchText = string.Empty;

        protected ListViewModel()
        {
            Items.CollectionChanged += (_, _) =>
                RefreshItems();
        }

        public void RefreshItems()
        {
            var filtered = Items
                .Where(item =>
                    item.Name.Contains(
                        searchText,
                        System.StringComparison.OrdinalIgnoreCase))
                .OrderBy(item => item.Name)
                .ToList();

            FilteredItems.Clear();

            foreach (var item in filtered)
            {
                FilteredItems.Add(item);
            }
        }

        public void AddItem(T item)
        {
            Items.Add(item);
        }

        public void RemoveItem(T item)
        {
            Items.Remove(item);
        }

        public void ReplaceItems(
            System.Collections.Generic.IEnumerable<T> items)
        {
            Items.Clear();

            foreach (var item in items)
            {
                Items.Add(item);
            }

            RefreshItems();
        }
    }
}
