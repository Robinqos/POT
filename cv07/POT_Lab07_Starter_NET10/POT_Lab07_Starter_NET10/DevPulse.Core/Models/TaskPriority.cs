namespace DevPulse.Core.Models;

/// <summary>
/// Priorita študijnej alebo vývojárskej úlohy.
/// Vo View sa pomocou IValueConverter transformuje na farbu odznaku (ViewModel tak nepozná konkrétny UI Brush).
/// </summary>
public enum TaskPriority
{
    Nizka,
    Stredna,
    Kriticka
}
