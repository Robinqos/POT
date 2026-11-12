using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Media;
using DevPulse.Core.Services;

namespace DevPulse.App.Services;

/// <summary>
/// Implementácia IDialogService pre Avalonia UI (prednáška MVVM, slajd 19).
/// Vytvára ľahké natívne modálne okno rešpektujúce aktuálnu Light/Dark tému bez závislosti od externých knižníc.
/// </summary>
public sealed class AvaloniaDialogService : IDialogService
{
    public async Task ShowInfoAsync(string title, string message)
    {
        var owner = GetMainWindow();
        if (owner is null)
            return;

        var okButton = new Button
        {
            Content = "OK",
            IsDefault = true,
            IsCancel = true,
            MinWidth = 90,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Right
        };

        var dialog = CreateDialogWindow(title, message, [okButton]);
        okButton.Click += (_, _) => dialog.Close();

        await dialog.ShowDialog(owner);
    }

    public async Task<bool> ConfirmAsync(string title, string message)
    {
        var owner = GetMainWindow();
        if (owner is null)
            return false;

        var yesButton = new Button
        {
            Content = "Áno",
            Classes = { "accent" },
            IsDefault = true,
            MinWidth = 90,
            HorizontalContentAlignment = HorizontalAlignment.Center
        };

        var noButton = new Button
        {
            Content = "Zrušiť",
            IsCancel = true,
            MinWidth = 90,
            HorizontalContentAlignment = HorizontalAlignment.Center
        };

        var dialog = CreateDialogWindow(title, message, [noButton, yesButton]);
        yesButton.Click += (_, _) => dialog.Close(true);
        noButton.Click += (_, _) => dialog.Close(false);

        return await dialog.ShowDialog<bool>(owner);
    }

    private static Window? GetMainWindow()
        => (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;

    private static Window CreateDialogWindow(string title, string message, IReadOnlyList<Button> buttons)
    {
        var buttonBar = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 8
        };

        foreach (var btn in buttons)
        {
            buttonBar.Children.Add(btn);
        }

        return new Window
        {
            Title = title,
            Width = 440,
            SizeToContent = SizeToContent.Height,
            CanResize = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Content = new Border
            {
                Classes = { "panel" },
                Margin = new Thickness(14),
                Child = new StackPanel
                {
                    Spacing = 12,
                    Children =
                    {
                        new TextBlock
                        {
                            Text = title,
                            FontSize = 15.5,
                            FontWeight = FontWeight.SemiBold,
                            TextWrapping = TextWrapping.Wrap
                        },
                        new TextBlock
                        {
                            Text = message,
                            TextWrapping = TextWrapping.Wrap,
                            FontSize = 13.5,
                            Opacity = 0.88
                        },
                        new Border
                        {
                            Margin = new Thickness(0, 4, 0, 0),
                            Child = buttonBar
                        }
                    }
                }
            }
        };
    }
}
