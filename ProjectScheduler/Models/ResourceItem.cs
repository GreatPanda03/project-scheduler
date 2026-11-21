using ProjectScheduler.Enums;
using System.Text.Json.Serialization;
using System;
using CommunityToolkit.Mvvm.ComponentModel;
using ProjectScheduler.Interfaces;

namespace ProjectScheduler.Models
{
    public partial class ResourceItem 
        : ObservableObject, IListItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [ObservableProperty]
        private string name = string.Empty;

        [ObservableProperty]
        private string description = string.Empty;

        [ObservableProperty]
        private ResourceType type;

        [ObservableProperty]
        private string location = string.Empty;
       
        [JsonIgnore]
        public string IconGlyph => Type switch
        {
            ResourceType.Document => "\uE8A5",
            ResourceType.Application => "\uECAA", // EB3B
            ResourceType.Website => "\uE774",
            _ => "\uE8A5"
        };

        partial void OnTypeChanged(
            ResourceType oldValue, ResourceType newValue)
        {
            OnPropertyChanged(nameof(IconGlyph));
        }
    }
}
