
using ProjectScheduler.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace ProjectScheduler.Services;

public static class ChecklistAccessState
{
    private static bool _isUnlocked;

    public static bool IsUnlocked
    {
        get => _isUnlocked;
        set
        {
            if (_isUnlocked == value)
                return;

            _isUnlocked = value;
            AccessChanged?.Invoke();
        }
    }

    public static ObservableCollection<PreWorkItem> ChecklistItems
    {
        get;
    } = new();

    public static bool IsChecklistLoaded { get; set; }

    public static event Action? AccessChanged;

    public static void UpdateAccess()
    {
        IsUnlocked =
            ChecklistItems.Count > 0 &&
            ChecklistItems.All(item => item.IsCompleted);
    }
}
