using CommunityToolkit.Mvvm.ComponentModel;
using ProjectScheduler.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace ProjectScheduler.Models
{
    public partial class ProjectItem
        : ObservableObject, IListItem, IHasResources, IHasTasks
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [ObservableProperty]
        private string name = string.Empty;

        [ObservableProperty]
        private string description = string.Empty;

        [ObservableProperty]
        private float priority = 0;

        [ObservableProperty]
        private string notes = string.Empty;

        public List<Guid> ResourceIds { get; set; } = new();

        public ObservableCollection<TaskItem> Tasks { get; set; } = new();

        // Convenience attributes
        [JsonIgnore]
        public int IncompleteTaskCount => CountTasks(Tasks, completed: false);

        [JsonIgnore]
        public string CreatedDateLabel => CreatedDate.ToString("G");

        [JsonIgnore]
        public string IncompleteTaskCountLabel => $"{IncompleteTaskCount} task(s) ongoing";


        public static int CountTasks(
            IEnumerable<TaskItem> tasks,
            bool completed) 
        {
            int count = 0;

            foreach (var task in tasks)
            {
                if (task.IsCompleted == completed)
                {
                    count++;
                }

                count += CountTasks(task.Subtasks, completed);
            }

            return count;
        }
    }    
}
