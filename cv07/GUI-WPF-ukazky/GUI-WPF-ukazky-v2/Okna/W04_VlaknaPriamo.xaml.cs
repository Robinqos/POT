using System.Windows;

namespace WpfUkazky.Okna;

// slajd 15 – zmena GUI z iného vlákna končí výnimkou InvalidOperationException
public partial class W04_VlaknaPriamo : Window
{
    public W04_VlaknaPriamo() {
        InitializeComponent();
    }
    private CancellationTokenSource? cts;

    private void button1_Click(object sender, RoutedEventArgs e)
    {
        if (cts == null) {
            cts = new CancellationTokenSource();
            var token = cts.Token;
            Task.Run(() => Pocitanie(token));
        }
    }

    private void button2_Click(object sender, RoutedEventArgs e)
    {
        cts?.Cancel();
        cts = null;
    }

    private void Pocitanie(CancellationToken token)
    {
        for (long i = 0; !token.IsCancellationRequested; i++) {
            double d = Math.Sin(i);
            if (i % 10 == 0) textBox1.Text = i.ToString(); // výnimka!
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        cts?.Cancel();
        base.OnClosed(e);
    }
}
