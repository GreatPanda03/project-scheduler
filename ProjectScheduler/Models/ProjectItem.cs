using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProjectScheduler.Models
{
    public class ProjectItem : INotifyPropertyChanged
    {

        // [[ ATTRIBUTES ]] //
        public Guid Id { get; set; } = Guid.NewGuid();

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public int IncompleteTaskCount => CountTasks(Tasks, completed: false);

        // Display labels for XAML convenience
        public string CreatedDateLabel => CreatedDate.ToString("G");
        public string IncompleteTaskCountLabel => $"{IncompleteTaskCount} task(s) ongoing";


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

        private float _priority = 0;
        public float Priority
        {
            get => _priority;
            set
            {
                if (_priority == value)
                {
                    return;
                }

                _priority = value;
                OnPropertyChanged();
            }
        }

        private string _notes = string.Empty;
        public string Notes
        {
            get => _notes;
            set
            {
                if (_notes == value)
                {
                    return;
                }

                _notes = value;
                OnPropertyChanged();
            }
        }

        public List<Guid> ResourceIds { get; set; } = new();

        public ObservableCollection<TaskItem> Tasks { get; set; } = new();


        // [[ METHODS ]] //

        public static int CountTasks(
            IEnumerable<TaskItem> tasks,
            bool completed) 
        {
            int count = 0;

            foreach (var task in tasks)
            {
                if (task.IsCompleted == completed) count++;
                count += CountTasks(task.Subtasks, completed);
            }

            return count;
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
