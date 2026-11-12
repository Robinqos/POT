using System.Windows;

namespace WpfUkazky.Okna;

// slajd 13 – obsluha udalosti (event handler) v kóde na pozadí
public partial class W02_CodeBehind : Window
{
    public W02_CodeBehind()
    {
        InitializeComponent();
    }

    private void BtnSpocitaj_Click(object sender, RoutedEventArgs e)
    {
        if (int.TryParse(txtCislo1.Text, out int a) &&
            int.TryParse(txtCislo2.Text, out int b))
            MessageBox.Show($"Súčet = {a + b}");
        else
            MessageBox.Show("Zadajte celé čísla.");
    }
}
