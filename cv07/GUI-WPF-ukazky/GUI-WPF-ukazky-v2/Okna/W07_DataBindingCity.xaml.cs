using System.Windows;
using WpfUkazky.ViewModels;

namespace WpfUkazky.Okna;

public partial class W07_DataBindingCity : Window
{
    public W07_DataBindingCity()
    {
        InitializeComponent();
        // v konštruktore okna (code-behind):
        DataContext = new MainWindowViewModel();
    }
}
