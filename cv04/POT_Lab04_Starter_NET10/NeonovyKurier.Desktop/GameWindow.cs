using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using NeonovyKurier.Core;

namespace NeonovyKurier.Desktop;

// Celá hra je pripravená. Rovnaké pravidlá používa aj konzolová záloha.
public sealed partial class GameWindow : Window
{
    private MissionSession _session = new(MissionDefinition.Training, isTraining: true);
    private string _runner = "@";
    private readonly TextBlock _name = new() { FontSize = 22, TextWrapping = TextWrapping.Wrap };
    private readonly TextBlock _kind = new() { FontSize = 13, TextWrapping = TextWrapping.Wrap };
    private readonly StackPanel _map = new() { Name = "Map", Orientation = Orientation.Horizontal, Spacing = 4 };
    private readonly TextBlock _progress = new() { Name = "GameProgress", FontSize = 14, TextWrapping = TextWrapping.Wrap };
    private readonly TextBlock _tip = new() { Name = "GameTip", FontSize = 12, TextWrapping = TextWrapping.Wrap };
    private readonly TextBlock _message = new() { Name = "GameMessage", FontSize = 14, TextWrapping = TextWrapping.Wrap };
    private readonly Button _step = new() { Name = "Step", Content = "Krok +1" };
    private readonly Button _jump = new() { Name = "Jump", Content = "Skok +2" };

    public MissionSession Session => _session;

    public GameWindow()
    {
        Title = "Neónový kuriér - hra";
        Width = 730; Height = 490; MinWidth = 560; MinHeight = 450;
        Background = Brush.Parse("#0B1220"); Foreground = Brushes.White;
        _step.Click += (_, _) => Move(1);
        _jump.Click += (_, _) => Move(2);
        var restart = new Button { Name = "Restart", Content = "Od začiatku" };
        restart.Click += (_, _) => { _session.Reset(); Update(); };
        Content = new ScrollViewer
        {
            Content = new StackPanel
            {
                Margin = new Thickness(24), Spacing = 14,
                Children =
                {
                    _name, _kind, _progress,
                    new ScrollViewer { Content = _map, HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto },
                    BuildLegend(),
                    new TextBlock { Text = "Krok je zadarmo. Skok stojí 1 energiu. Všetky zásielky musíš zobrať dopadom.", FontSize = 13, TextWrapping = TextWrapping.Wrap },
                    _message,
                    new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10,
                        Children = { _step, _jump, restart } },
                    _tip
                }
            }
        };
        LoadMission(MissionDefinition.Training, "@", "V tréningu skús Krok, Krok, Krok alebo Skok, Krok.", isTraining: true);
    }

    public void LoadMission(MissionDefinition definition, string runner, string hint, bool isTraining = false)
    {
        _session = new MissionSession(definition, isTraining);
        _runner = runner;
        _name.Text = definition.Name;
        _kind.Text = isTraining
            ? "TRÉNING OVLÁDANIA. Hlavná misia čaká na dokončenie U2-U4."
            : "HLAVNÁ MISIA. Doruč všetky zásielky s dostupnou energiou.";
        _tip.Text = hint;
        Update();
    }

    private void Move(int distance) { _session.Move(distance); Update(); }

    private void Update()
    {
        _message.Text = _session.Message;
        _step.IsEnabled = _jump.IsEnabled = !_session.Finished;
        RenderTrack();
    }
}
