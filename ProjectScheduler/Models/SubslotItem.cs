using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProjectScheduler.Models
{
    internal class SubslotItem : INotifyPropertyChanged
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        private string _name = string.Empty;
        public string Name
        {
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
        
        private bool _lockResources = false;
        public bool LockResources
        {
            get => _lockResources;
            set
            {
                if (_lockResources == value)
                {
                    return;
                }

                _lockResources = value;
                OnPropertyChanged();
            }
        }

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
