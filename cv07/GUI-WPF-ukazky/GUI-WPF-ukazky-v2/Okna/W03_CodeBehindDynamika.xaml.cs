using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WpfUkazky.Okna;

// slajd 14 – udalosti pripojené v konštruktore po InitializeComponent()
public partial class W03_CodeBehindDynamika : Window
{
    public W03_CodeBehindDynamika()
    {
        InitializeComponent();
        btnAno.MouseEnter += BtnAno_MouseEnter;
        btnNie.Click += BtnNie_Click;
    }

    private void BtnAno_MouseEnter(object sender, MouseEventArgs e)
    {
        double x = Canvas.GetLeft(btnAno);
        Canvas.SetLeft(btnAno, x < 50 ? 190 : 10);
    }

    private void BtnNie_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Tak seď a dávaj pozor!");
    }
}
