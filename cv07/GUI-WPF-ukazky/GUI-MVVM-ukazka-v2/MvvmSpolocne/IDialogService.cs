namespace MvvmSpolocne;

// slajd 19 – rozhranie: ViewModel cez neho požiada o dialóg a nevie nič o WPF
public interface IDialogService
{
    void ShowMessage(string text, string title = "");
    bool Confirm(string text, string title = "");
}
