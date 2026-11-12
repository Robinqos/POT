using System.Windows;
using System.Windows.Controls;

namespace WpfUkazky;

public partial class MainWindow : Window
{
    public MainWindow() => InitializeComponent();

    // Tag tlačidla obsahuje typ okna (x:Type v XAML) – jedna obsluha pre všetky ukážky
    private void Otvor_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: Type typ } && Activator.CreateInstance(typ) is Window okno)
        {
            okno.Owner = this;
            okno.ShowDialog();
        }
    }
}
