using CommunityToolkit.Mvvm.ComponentModel;
using DevPulse.Core.Models;

namespace DevPulse.Core.ViewModels;

/// <summary>
/// ViewModel jednej úlohy v študijnom / vývojárskom TODO zozname.
/// </summary>
public partial class TaskItemViewModel : ViewModelBase
{
    private readonly Action? _onStateChanged;

    public TaskItemViewModel(string title, TaskPriority priority = TaskPriority.Stredna, bool isCompleted = false, string? sourceUrl = null, Action? onStateChanged = null)
    {
        this.title = title;
        this.priority = priority;
        this.isCompleted = isCompleted;
        SourceUrl = sourceUrl;
        _onStateChanged = onStateChanged;
    }

    [ObservableProperty]
    private string title;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PriorityLabel))]
    private TaskPriority priority;

    [ObservableProperty]
    private bool isCompleted;

    public string PriorityLabel => Priority switch
    {
        TaskPriority.Nizka => "Nízka",
        TaskPriority.Stredna => "Stredná",
        TaskPriority.Kriticka => "Kritická",
        _ => Priority.ToString()
    };

    public string? SourceUrl { get; }

    public bool HasSourceUrl => !string.IsNullOrWhiteSpace(SourceUrl)
                                && !SourceUrl.StartsWith("offline://", StringComparison.OrdinalIgnoreCase);

    partial void OnIsCompletedChanged(bool value) => _onStateChanged?.Invoke();
    partial void OnPriorityChanged(TaskPriority value) => _onStateChanged?.Invoke();
}
