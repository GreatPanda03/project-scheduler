using CommunityToolkit.Mvvm.ComponentModel;
using ProjectScheduler.Interfaces;
using ProjectScheduler.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace ProjectScheduler.ViewModels
{
    internal sealed partial class TaskViewModel
        : ObservableObject
    {
        // NOTE: no service used, just indexes Target's Tasks directly
        // * saving is performed by the service which owns the tasks
        // * eg. if Target is ProjectItem, use ProjectService to save

        [ObservableProperty]
        private IHasTasks? _target;

        [ObservableProperty]
        private TaskItem? _selectedTask;

        public ObservableCollection<TaskListEntry> FilteredTasks { get; } = new();

        private readonly HashSet<Guid> _collapsedTaskIds = new();

        [ObservableProperty]
        private int completionFilter;

        [ObservableProperty]
        private int dateSort;


        public TaskViewModel()
        {
            completionFilter = 0;
            dateSort = 0;
        }



        // [[ FILTERING METHODS ]] //
        partial void OnCompletionFilterChanged(int oldValue, int newValue)
        {
            RefreshTasks();
        }

        partial void OnDateSortChanged(int oldValue, int newValue)
        {
            RefreshTasks();
        }
        

        
        // [[ ADDING TASKS ]] //

        public bool AddTask(TaskItem task)
        {
            if (Target is null) return false;
            
            if (!Target.Tasks.Contains(task))
            {
                Target.Tasks.Add(task);
                return true;
            }

            return false;
        }

        public bool AddSubtask(TaskItem parent, TaskItem subtask)
        {
            if (!parent.Subtasks.Contains(subtask))
            {
                parent.AddSubtask(subtask);
                return true;
            }

            return false;
        }



        // [[ DELETING TASKS ]] //

        public bool DeleteTask(TaskItem task)
        {
            if (Target is null) return false;

            var siblings = task.Parent?.Subtasks ?? Target.Tasks;

            bool removed = siblings.Remove(task);

            if (SelectedTask == task)
            {
                SelectedTask = null;
            }

            return removed;
        }



        // [[ REFRESH TASKS ]] //

        public void RefreshTasks()
        {
            FilteredTasks.Clear();

            if (Target is null) return;

            var allTasks = new List<TaskListEntry>();
            bool descending = DateSort == 1;

            AddTaskAndSubtasksToList(
                Target.Tasks, 0, allTasks, descending);

            IEnumerable<TaskListEntry> filteredTasks = allTasks;

            // Completion filter (All = 0, Complete, Incomplete)
            if (CompletionFilter == 1)
            {
                filteredTasks = filteredTasks.Where(
                    entry => entry.Task.IsCompleted);
            }
            else if (CompletionFilter == 2)
            {
                filteredTasks = filteredTasks.Where(
                    entry => !entry.Task.IsCompleted);
            }

            // * NOTE: Date sorting done in AddTaskAndSubtasksToList()

            // Refreshing the list
            foreach (var entry in filteredTasks)
            {
                FilteredTasks.Add(entry);
            }
        }



        // [[ TASK LISTING & HIEARCHY ]] //

        public void ToggleTaskExpansion(Guid taskId)
        {
            // Toggle the task's collapsed state
            if (!_collapsedTaskIds.Remove(taskId))
            {
                _collapsedTaskIds.Add(taskId);
            }

            // Refresh the visible task hierarchy
            RefreshTasks();
        }

        private void AddTaskAndSubtasksToList(
            IEnumerable<TaskItem> tasks,
            int depth,
            List<TaskListEntry> entries,
            bool descending)
        {
            // Sort only tasks that share the same parent
            var orderedTasks = descending
                ? tasks.OrderByDescending(task => task.CreatedDate)
                : tasks.OrderBy(task => task.CreatedDate);

            foreach (var task in orderedTasks)
            {
                bool isCollapsed = _collapsedTaskIds.Contains(task.Id);

                // Create the entry with the appropriate expansion glyph
                TaskListEntry entry = new(task, depth)
                {
                    ExpandGlyph = isCollapsed ? "\uE76C" : "\uE70D"
                };

                // Add the parent task first, then its subtasks (depth-first traversal)
                // * only add Subtasks if the parent is expanded
                entries.Add(entry);

                if (!isCollapsed)
                {
                    AddTaskAndSubtasksToList(
                        task.Subtasks, depth + 1, entries, descending);
                }
            }
        }
    }
}
