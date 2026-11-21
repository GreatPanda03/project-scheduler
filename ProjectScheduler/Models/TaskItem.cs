using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProjectScheduler.Models
{
    public class TaskItem : INotifyPropertyChanged
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set
            {
                if (_name == value) return;
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
                if (_description == value) return;
                _description = value;
                OnPropertyChanged();
            }
        }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public bool _isCompleted;
        public bool IsCompleted
        {
            get => _isCompleted;
            set
            {
                if (_isCompleted == value) return;
                _isCompleted = value;
                CompletedDate = value ? DateTime.Now : null;

                OnPropertyChanged();
                OnPropertyChanged(nameof(CompletedDate));
            }
        }

        public DateTime? CompletedDate { get; set; }

        public ObservableCollection<TaskItem> Subtasks { get; set; } = new();

        public bool AddSubtask(TaskItem subtask)
        {
            if (subtask == null || Subtasks.Contains(subtask))
            {
                return false;
            }

            Subtasks.Add(subtask);
            OnPropertyChanged(nameof(Subtasks));
            return true;
        }

        public bool RemoveSubtask(TaskItem subtask)
        {
            if (subtask == null || !Subtasks.Remove(subtask))
            {
                return false;
            }

            OnPropertyChanged(nameof(Subtasks));
            return true;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(
            [CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));
        }
    }
}
