using System.Windows;

namespace WpfUkazky.Okna;

// slajd 16 – aktualizácia GUI sa prepošle do UI vlákna cez Dispatcher
public partial class W05_VlaknaDispatcher : Window
{
    public W05_VlaknaDispatcher() {
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
        for (long i = 0; !token.IsCancellationRequested; i++)
        {
            double d = Math.Sin(i);
            if (i % 10 == 0)
                Dispatcher.Invoke(() => textBox1.Text = i.ToString());
        }
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        // zastaviť výpočet skôr, než okno zanikne (Dispatcher.Invoke by inak čakal na zatvorené okno)
        cts?.Cancel();
        base.OnClosing(e);
    }
}
