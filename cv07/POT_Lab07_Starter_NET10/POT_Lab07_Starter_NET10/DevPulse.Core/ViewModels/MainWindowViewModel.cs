using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevPulse.Core.Models;
using DevPulse.Core.Services;

namespace DevPulse.Core.ViewModels;

/// <summary>
/// Štartovacia kostra hlavného ViewModelu pre Laboratórne cvičenie 7 (Lab 07).
/// Doplňte časti označené komentárom TODO (prvú úlohu spravte ručne, na ďalšie môžete využiť AI asistenta so súborom AGENTS.md).
/// </summary>
public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IFeedService _feedService;
    private readonly IDialogService _dialogService;
    private readonly IBrowserLauncher _browserLauncher;

    public MainWindowViewModel()
        : this(new HttpFeedService(), new DesignTimeDialogService(), new SystemBrowserLauncher())
    {
    }

    public MainWindowViewModel(IFeedService feedService, IDialogService dialogService, IBrowserLauncher browserLauncher)
    {
        _feedService = feedService;
        _dialogService = dialogService;
        _browserLauncher = browserLauncher;

        Sources = _feedService.GetDefaultSources();
        selectedSource = Sources.FirstOrDefault();

        Tasks.Add(CreateTask("Preskúmať preklad WPF XAML → Avalonia AXAML", TaskPriority.Nizka, isCompleted: true));
        Tasks.Add(CreateTask("Doplniť MVVM príkazy a asynchrónnu analýzu", TaskPriority.Kriticka));

        Tasks.CollectionChanged += (_, _) => NotifyTaskStatsChanged();

        foreach (var demo in HttpFeedService.GetOfflineArticles())
        {
            Articles.Add(demo);
        }
    }

    public IReadOnlyList<FeedSource> Sources { get; }
    public IReadOnlyList<TaskPriority> Priorities { get; } = Enum.GetValues<TaskPriority>();

    public ObservableCollection<FeedArticle> Articles { get; } = [];
    public ObservableCollection<TaskItemViewModel> Tasks { get; } = [];

    // TODO [Úloha 3 / Fáza 3]: Po vytvorení príkazu LoadFeedAsync pridajte nad pole selectedSource atribút
    // [NotifyCanExecuteChangedFor(nameof(LoadFeedCommand))]
    [ObservableProperty]
    private FeedSource? selectedSource;

    // TODO [Úloha 2 / Fáza 2]: Pridajte atribút [NotifyCanExecuteChangedFor(nameof(AddTaskCommand))],
    // aby sa tlačidlo „Pridať“ automaticky sprístupnilo/zablokovalo pri písaní textu.
    [ObservableProperty]
    private string newTaskTitle = "";

    [ObservableProperty]
    private TaskPriority selectedPriority = TaskPriority.Stredna;

    // TODO [Úloha 2 / Fáza 2]: Po vytvorení príkazu RemoveSelectedTask pridajte nad pole selectedTask atribút
    // [NotifyCanExecuteChangedFor(nameof(RemoveSelectedTaskCommand))]
    [ObservableProperty]
    private TaskItemViewModel? selectedTask;

    [ObservableProperty]
    private string statusMessage = "Štartovacia kostra pripravená. Postupujte podľa návodu v POT_Lab07_Navod.docx.";

    [ObservableProperty]
    private double progressPercent;

    // TODO [Úloha 3 / Fáza 3]: Po vytvorení príkazov LoadFeedAsync a AnalyzeFeedAsync pridajte nad pole isBusy atribúty
    // [NotifyCanExecuteChangedFor(nameof(LoadFeedCommand))] a [NotifyCanExecuteChangedFor(nameof(AnalyzeFeedCommand))]
    [ObservableProperty]
    private bool isBusy;

    public int CompletedTasksCount => Tasks.Count(t => t.IsCompleted);

    public int PendingCriticalCount => Tasks.Count(t => !t.IsCompleted && t.Priority == TaskPriority.Kriticka);

    public string TaskStatsText =>
        $"Hotovo {CompletedTasksCount}/{Tasks.Count}  ·  Kritické: {PendingCriticalCount}";

    // TODO [Úloha 2 / Fáza 2 – Ručne bez AI]:
    // Implementujte metódu CanAddTask() => !string.IsNullOrWhiteSpace(NewTaskTitle)
    // a príkaz [RelayCommand(CanExecute = nameof(CanAddTask))] private void AddTask(),
    // ktorý pridá novú úlohu na začiatok kolekcie cez CreateTask(NewTaskTitle.Trim(), SelectedPriority) a vyčistí NewTaskTitle = "".

    // TODO [Úloha 2 / Fáza 2]:
    // Implementujte príkaz [RelayCommand] private void AddArticleToTasks(FeedArticle? article),
    // ktorý pridá vybraný článok z RSS čítačky medzi úlohy: CreateTask($"Prečítať: {article.Title}", TaskPriority.Stredna, sourceUrl: article.Link).

    // TODO [Úloha 2 / Fáza 2]:
    // Implementujte metódu private bool CanRemoveSelectedTask() => SelectedTask is not null;,
    // príkaz [RelayCommand(CanExecute = nameof(CanRemoveSelectedTask))] private void RemoveSelectedTask()
    // a asynchrónny príkaz [RelayCommand] private async Task ClearCompletedTasksAsync(),
    // ktorý si pred vymazaním splnených úloh vypýta súhlas cez:
    // var confirmed = await _dialogService.ConfirmAsync(...); if (!confirmed) return;

    // TODO [Úloha 3 / Fáza 3]:
    // 1. Implementujte metódu private bool CanLoadFeed() => SelectedSource is not null && !IsBusy;
    //    a príkaz [RelayCommand(CanExecute = nameof(CanLoadFeed), IncludeCancelCommand = true)]
    //    private async Task LoadFeedAsync(CancellationToken cancellationToken),
    //    ktorý načíta články zo SelectedSource.Url cez _feedService.LoadFeedAsync(...) do kolekcie Articles
    //    (vo finally nastavte IsBusy = false a zavolajte AnalyzeFeedCommand.NotifyCanExecuteChanged()).
    // 2. Implementujte metódu private bool CanAnalyzeFeed() => Articles.Count > 0 && !IsBusy;
    //    a príkaz [RelayCommand(CanExecute = nameof(CanAnalyzeFeed), IncludeCancelCommand = true)]
    //    private async Task AnalyzeFeedAsync(CancellationToken cancellationToken),
    //    ktorý vytvorí new Progress<AnalysisProgress>(...) chránený lokálnym zámkom/príznakom isActive
    //    a zavolá await _feedService.AnalyzeArticlesAsync(snapshot, progress, cancellationToken).
    //    Nezabudnite zachytiť OperationCanceledException a vo finally vrátiť IsBusy = false!

    // TODO [Bonus – „Ideme na pivo?“]:
    // Pridajte vlastnosť public bool HasPendingCriticalTasks => PendingCriticalCount > 0;
    // a príkaz [RelayCommand] private async Task GoForBeerAsync() využívajúci _dialogService.ShowInfoAsync(...).

    [RelayCommand]
    private void OpenArticle(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return;

        if (url.StartsWith("offline://", StringComparison.OrdinalIgnoreCase))
        {
            StatusMessage = "Toto je lokálny offline ukážkový článok – prepnite na online RSS zdroj pre otvorenie v prehliadači.";
            return;
        }

        if (!SystemBrowserLauncher.IsSafeWebUrl(url, out var safeUri) || safeUri is null)
        {
            StatusMessage = "Nepodporovaný alebo neplatný odkaz článku (povolené sú iba http/https URL).";
            return;
        }

        try
        {
            _browserLauncher.OpenUrl(safeUri.AbsoluteUri);
            StatusMessage = $"Otvorený odkaz v prehliadači: {safeUri.AbsoluteUri}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Nepodarilo sa otvoriť prehliadač: {ex.Message}";
        }
    }

    private TaskItemViewModel CreateTask(string title, TaskPriority priority, bool isCompleted = false, string? sourceUrl = null)
        => new(title, priority, isCompleted, sourceUrl, NotifyTaskStatsChanged);

    private void NotifyTaskStatsChanged()
    {
        OnPropertyChanged(nameof(CompletedTasksCount));
        OnPropertyChanged(nameof(PendingCriticalCount));
        OnPropertyChanged("HasPendingCriticalTasks");
        OnPropertyChanged(nameof(TaskStatsText));
    }

    private sealed class DesignTimeDialogService : IDialogService
    {
        public Task ShowInfoAsync(string title, string message) => Task.CompletedTask;
        public Task<bool> ConfirmAsync(string title, string message) => Task.FromResult(true);
    }
}
