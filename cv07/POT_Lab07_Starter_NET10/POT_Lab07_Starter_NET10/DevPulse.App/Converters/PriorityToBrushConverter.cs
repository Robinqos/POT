using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using DevPulse.Core.Models;

namespace DevPulse.App.Converters;

/// <summary>
/// TODO [Úloha 2.1 / Fáza 2]: Upravte metódu Convert tak, aby vracala odlišnú farbu (IBrush)
/// pre TaskPriority.Nizka (#059669), Stredna (#D97706) a Kriticka (#DC2626).
/// </summary>
public sealed class PriorityToBrushConverter : IValueConverter
{
    private static readonly IBrush DefaultBrush = new SolidColorBrush(Color.Parse("#64748B"));

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // TODO [Úloha 2.1 / Fáza 2]: Nahraďte switch výrazom podľa hodnoty TaskPriority
        // (Nizka -> "#059669", Stredna -> "#D97706", Kriticka -> "#DC2626")
        _ = value as TaskPriority?;
        return DefaultBrush;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Pomocný konvertor na zobrazenie hodnôt enumu TaskPriority so slovenskou diakritikou (napr. v ComboBoxe).
/// </summary>
public sealed class PriorityLabelConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value switch
        {
            TaskPriority.Nizka => "Nízka",
            TaskPriority.Stredna => "Stredná",
            TaskPriority.Kriticka => "Kritická",
            _ => value?.ToString() ?? ""
        };

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
