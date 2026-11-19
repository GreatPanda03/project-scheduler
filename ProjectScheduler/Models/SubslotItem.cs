using CommunityToolkit.Mvvm.ComponentModel;
using ProjectScheduler.Interfaces;
using System;

namespace ProjectScheduler.Models
{
    public partial class SubslotItem : ObservableObject, IListItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [ObservableProperty]
        private string name = string.Empty;

        [ObservableProperty]
        private string description = string.Empty;

        [ObservableProperty]
        private bool lockResources;
    }
}
