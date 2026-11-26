using ProjectScheduler.Models;
using System.Collections.ObjectModel;

namespace ProjectScheduler.Interfaces
{
    public interface IHasTasks
    {
        ObservableCollection<TaskItem> Tasks { get; }
    }
}
