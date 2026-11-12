using System.Windows;
using MvvmToolkit.ViewModels;

namespace MvvmToolkit.Views;

// ViewModel dodá DI kontajner cez konštruktor
public partial class ShellView : Window
{
    public ShellView(ShellViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}
