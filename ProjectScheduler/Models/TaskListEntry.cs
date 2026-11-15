using Thickness = Microsoft.UI.Xaml.Thickness;

namespace ProjectScheduler.Models
{
    public class TaskListEntry
    {
        public TaskItem Task { get; set; }

        public int Depth { get; set; }

        public Thickness IndentMargin =>
            new Thickness(Depth * 24, 0, 0, 0);

        public string DateLabel =>
            Task.CompletedDate?.ToString("d")
            ?? Task.CreatedDate.ToString("d");

        public TaskListEntry(TaskItem task, int depth)
        {
            Task = task;
            Depth = depth;
        }
    }
}
