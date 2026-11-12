using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using DevPulse.Core.ViewModels;

namespace DevPulse.App.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void ThemeSwitch_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (Application.Current is null || sender is not ToggleSwitch toggle)
            return;

        Application.Current.RequestedThemeVariant = toggle.IsChecked == true
            ? ThemeVariant.Dark
            : ThemeVariant.Light;
    }

    // TODO [Bonus – voliteľne]: Pre tlačidlo „Idem na pivo“ doplňte obsluhu
    // private void BeerButton_OnPointerEntered(object? sender, PointerEventArgs e),
    // ktorá pri vm.HasPendingCriticalTasks plynule posunie TranslateTransform.X o -130 px.
}
