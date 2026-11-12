using System.Windows;

namespace WpfUkazky.Okna;

// slajd 17 – Task.Run + async/await + Progress<T> + CancellationToken
public partial class W06_VlaknaAsync : Window
{
    public W06_VlaknaAsync() => InitializeComponent();

    private CancellationTokenSource? cts;

    private async void button1_Click(object sender, RoutedEventArgs e)
    {
        if (cts != null) return;
        cts = new CancellationTokenSource();
        var token = cts.Token;
        var progress = new Progress<long>(i => textBox1.Text = i.ToString());
        try {
            await Task.Run(() => Pocitanie(progress, token), token);
        }
        catch (OperationCanceledException) {
            textBox1.Text += " (zastavené)";
        }
        finally {
            cts.Dispose();
            cts = null;
        }
    }

    private void button2_Click(object sender, RoutedEventArgs e) => cts?.Cancel();

    private static void Pocitanie(IProgress<long> progress, CancellationToken token)
    {
        for (long i = 0; ; i++) {
            token.ThrowIfCancellationRequested();
            double d = Math.Sin(i);
            if (i % 100_000 == 0) progress.Report(i);
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        cts?.Cancel();
        base.OnClosed(e);
    }
}
