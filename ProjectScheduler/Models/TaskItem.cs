using CommunityToolkit.Mvvm.ComponentModel;
using ProjectScheduler.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace ProjectScheduler.Models
{
    public partial class TaskItem
        : ObservableObject, IListItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [ObservableProperty]
        private string name = string.Empty;

        [ObservableProperty]
        private string description = string.Empty;

        [ObservableProperty]
        private bool isCompleted;

        public DateTime? CompletedDate { get; set; }

        public ObservableCollection<TaskItem> Subtasks { get; set; } = new();

        [JsonIgnore]
        public TaskItem? Parent { get; set; }

        partial void OnIsCompletedChanged(bool value)
        {
            CompletedDate = value ? DateTime.Now : null;
            OnPropertyChanged(nameof(CompletedDate));
        }


        public bool AddSubtask(TaskItem subtask)
        {
            if (subtask == null || Subtasks.Contains(subtask))
            {
                return false;
            }

            subtask.Parent = this;
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
    }
}
