using ProjectScheduler.Enums;
using System.Text.Json.Serialization;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProjectScheduler.Models
{
    public class ResourceItem : INotifyPropertyChanged
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        private string _name = string.Empty;
        public string Name {
            get => _name;
            set
            {
                if (_name == value)
                {
                    return;
                }

                _name = value;
                OnPropertyChanged();
            }
        }

        private string _description = string.Empty;
        public string Description
        {
            get => _description;
            set
            {
                if (_description == value)
                {
                    return;
                }

                _description = value;
                OnPropertyChanged();
            }
        }

        private ResourceType _type;
        public ResourceType Type
        {
            get => _type;
            set
            {
                if (_type == value)
                {
                    return;
                }

                _type = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(IconGlyph));
            }
        }

        private string _directory = string.Empty;
        public string Directory
        {
            get => _directory;
            set
            {
                if (_directory == value)
                {
                    return;
                }

                _directory = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public string IconGlyph => Type switch
        {
            ResourceType.Document => "\uE8A5",
            ResourceType.Application => "\uECAA", // EB3B
            ResourceType.Website => "\uE774",
            _ => "\uE8A5"
        };

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(
            [CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName)
            );
        }
    }
}
