using NeonovyKurier.Core;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia.Media;

namespace NeonovyKurier.Desktop;

// Pripravené vykreslenie políčok. Level ostáva krátky text v JSON súbore.
public sealed partial class GameWindow
{
    private void RenderTrack()
    {
        _map.Children.Clear();
        _progress.Text = $"Políčko {_session.Position + 1} z {_session.Mission.Track.Length}  |  Zásielky {_session.Collected.Count}/{_session.TotalParcels}  |  Energia {_session.JumpsLeft}/{_session.Mission.JumpBudget}";
        for (int i = 0; i < _session.Mission.Track.Length; i++)
        {
            var cell = new Border
            {
                Width = 34, Height = 48, CornerRadius = new CornerRadius(6),
                Background = Brush.Parse(i < _session.Position ? "#243349" : "#172238"),
                BorderBrush = i == _session.Position ? Brushes.White : Brush.Parse("#334155"),
                BorderThickness = new Thickness(i == _session.Position ? 2 : 1)
            };
            Control? symbol = _session.Mission.Track[i] switch
            {
                '#' => Wall(),
                '*' when !_session.Collected.Contains(i) => Parcel(),
                '*' => new TextBlock { Text = "✓", FontSize = 16, Foreground = Brushes.LightGreen },
                'G' => Goal(),
                'S' => new TextBlock { Text = "S", FontSize = 12, Foreground = Brushes.LightGray },
                _ => null
            };
            if (i == _session.Position)
            {
                symbol = Player();
                symbol.Name = "Player";
                AutomationProperties.SetName(symbol, $"Hráč {_runner}, políčko {i + 1}");
            }
            if (symbol is not null)
            {
                symbol.HorizontalAlignment = HorizontalAlignment.Center;
                symbol.VerticalAlignment = VerticalAlignment.Center;
                cell.Child = symbol;
            }
            string kind = _session.Mission.Track[i] switch { '#' => "stena", '*' => "zásielka", 'G' => "cieľ", 'S' => "štart", _ => "voľné" };
            ToolTip.SetTip(cell, $"Políčko {i + 1}: {kind}");
            _map.Children.Add(cell);
        }
    }

    private Control Player()
    {
        var face = new Grid { Width = 24, Height = 24 };
        face.Children.Add(new Ellipse { Fill = Brush.Parse("#60A5FA") });
        if (_runner == "@")
        {
            face.Children.Add(new Ellipse { Width = 3, Height = 3, Fill = Brushes.Black, Margin = new Thickness(-8, -4, 0, 0) });
            face.Children.Add(new Ellipse { Width = 3, Height = 3, Fill = Brushes.Black, Margin = new Thickness(8, -4, 0, 0) });
        }
        else face.Children.Add(new TextBlock
        {
            Text = _runner, FontSize = 15, FontWeight = FontWeight.Bold, Foreground = Brushes.Black,
            HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center
        });
        return face;
    }

    private static Control Wall() => new Border
    {
        Width = 22, Height = 24, CornerRadius = new CornerRadius(3), Background = Brush.Parse("#EF6666"),
        Child = new TextBlock { Text = "#", Foreground = Brushes.Black, FontWeight = FontWeight.Bold,
            HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center }
    };

    private static Control Parcel() => new Border
    {
        Width = 20, Height = 20, CornerRadius = new CornerRadius(3), Background = Brush.Parse("#FBBF24"),
        Child = new TextBlock { Text = "+", FontSize = 16, FontWeight = FontWeight.Bold, Foreground = Brushes.Black,
            HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center }
    };

    private static Control Goal()
    {
        var flag = new Grid { Width = 22, Height = 26 };
        flag.Children.Add(new Rectangle { Width = 3, Fill = Brushes.White, HorizontalAlignment = HorizontalAlignment.Left });
        flag.Children.Add(new Border { Width = 17, Height = 12, Background = Brush.Parse("#4ADE80"),
            HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Top });
        return flag;
    }

    private Control BuildLegend()
    {
        var legend = new WrapPanel();
        foreach (var (icon, name) in new[] { (Player(), "hráč"), (Wall(), "stena"), (Parcel(), "zásielka"), (Goal(), "cieľ") })
        {
            legend.Children.Add(new StackPanel
            {
                Orientation = Orientation.Horizontal, Spacing = 7, Margin = new Thickness(0, 0, 18, 4),
                Children = { icon, new TextBlock { Text = name, FontSize = 13, VerticalAlignment = VerticalAlignment.Center } }
            });
        }
        return legend;
    }
}
