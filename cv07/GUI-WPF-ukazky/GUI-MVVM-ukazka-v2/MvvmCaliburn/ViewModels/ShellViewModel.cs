using Caliburn.Micro;
using MvvmSpolocne;

namespace MvvmCaliburn.ViewModels;

// slajd 18 – konvencie: TxtMeno ↔ x:Name="TxtMeno", CmdSave ↔ tlačidlo, CanCmdSave ↔ IsEnabled
// slajd 19 – dialógy cez IDialogService namiesto MessageBox
public class ShellViewModel(IDialogService dialogs) : Screen
{
    private string _meno = "", _prie = "", _mail = "";

    public string TxtMeno
    {
        get => _meno;
        set { Set(ref _meno, value); Zmena(); }
    }
    public string TxtPrie
    {
        get => _prie;
        set { Set(ref _prie, value); Zmena(); }
    }
    public string TxtMail
    {
        get => _mail;
        set { Set(ref _mail, value); Zmena(); }
    }
    private void Zmena() => NotifyOfPropertyChange(nameof(CanCmdSave));

    public bool CanCmdSave =>
        !string.IsNullOrWhiteSpace(TxtMeno) &&
        !string.IsNullOrWhiteSpace(TxtPrie) &&
        TxtMail.Contains('@');

    public void CmdSave()
    {
        if (dialogs.Confirm($"Uložiť {TxtMeno} {TxtPrie}?"))
            dialogs.ShowMessage("Uložené.");
    }

    public Task CmdExit() => TryCloseAsync();
}
