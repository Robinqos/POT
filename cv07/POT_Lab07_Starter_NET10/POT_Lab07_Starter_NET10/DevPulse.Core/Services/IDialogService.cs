namespace DevPulse.Core.Services;

/// <summary>
/// Abstrakcia dialógov pre ViewModel (prednáška MVVM, slajd 19).
/// V multiplatformových GUI frameworkoch (Avalonia, MAUI, Blazor) sú dialógy asynchrónne (Task),
/// aby neblokovali UI vlákno. Vo WPF sa dajú obaliť cez Task.FromResult, v unit testoch cez FakeDialogService.
/// </summary>
public interface IDialogService
{
    Task ShowInfoAsync(string title, string message);
    Task<bool> ConfirmAsync(string title, string message);
}
