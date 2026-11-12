using System.Windows;

namespace MvvmSpolocne;

// slajd 19 – implementácia: patrí do vrstvy View, smie používať WPF
public class DialogService : IDialogService
{
    public void ShowMessage(string text, string title = "")
        => MessageBox.Show(text, title);

    public bool Confirm(string text, string title = "")
        => MessageBox.Show(text, title, MessageBoxButton.YesNo)
           == MessageBoxResult.Yes;
}
