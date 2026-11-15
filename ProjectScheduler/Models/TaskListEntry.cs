using Microsoft.UI.Xaml;
using System;
using System.Collections;
using System.Collections.Generic;
using Thickness = Microsoft.UI.Xaml.Thickness;

namespace ProjectScheduler.Models
{
    public class TaskListEntry
    {
        public TaskItem Task { get; set; }

        public int Depth { get; set; }

        public bool HasSubtasks => Task.Subtasks.Count > 0;

        public Visibility ExpandVisibility =>
            HasSubtasks ? Visibility.Visible : Visibility.Collapsed;

        public int TotalSubtaskCount =>
            CountSubtasks(Task.Subtasks, incompleteOnly: false);

        public int IncompleteSubtaskCount =>
            CountSubtasks(Task.Subtasks, incompleteOnly: true);

        // XAML convenience attributes
        public string SubtaskProgress => HasSubtasks
            ? $"{TotalSubtaskCount - IncompleteSubtaskCount}/{TotalSubtaskCount} task(s) complete"
            : string.Empty;

        public string ExpandGlyph { get; set; } = "\uE76C";

        public Thickness IndentMargin =>
            new Thickness(Depth * 12, 0, 0, 0);

        public string DateLabel =>
            Task.CompletedDate?.ToString("G") != null
                ? $"{Task.CreatedDate.ToString("G")}   (completed {Task.CompletedDate?.ToString("G")})"
                : Task.CreatedDate.ToString("G");


        // [[ CONSTRUCTOR ]] //

        public TaskListEntry(TaskItem task, int depth)
        {
            Task = task;
            Depth = depth;
        }


        // [[ METHODS ]] //
        private static int CountSubtasks(
            IEnumerable<TaskItem> tasks,
            bool incompleteOnly)
        {
            int count = 0;

            foreach (var task in tasks)
            {
                if (!incompleteOnly || !task.IsCompleted)
                {
                    count++;
                }

                count += CountSubtasks(
                    task.Subtasks,
                    incompleteOnly);
            }

            return count;
        }
    }
}
