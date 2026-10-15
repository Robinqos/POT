using System.Windows;

namespace KST.POT.AsyncExamples.U07bWpfSyncContext;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        this.InitializeComponent();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        Console.WriteLine("SynchronizationContext.Current = " + (SynchronizationContext.Current?.ToString() ?? "(null)"));
    }
}
