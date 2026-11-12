using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MvvmSpolocne;

namespace MvvmToolkit.ViewModels;

// slajd 20 – ten istý formulár ako v Caliburn.Micro, zvyšok kódu vygeneruje kompilátor
public partial class ShellViewModel(IDialogService dialogs) : ObservableObject
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(UlozCommand))]
    private string meno = "";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(UlozCommand))]
    private string priezvisko = "";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(UlozCommand))]
    private string mail = "";

    private bool MozemUlozit() =>
        Meno.Length > 0 && Priezvisko.Length > 0 && Mail.Contains('@');

    [RelayCommand(CanExecute = nameof(MozemUlozit))]
    private void Uloz()
    {
        if (dialogs.Confirm($"Uložiť {Meno} {Priezvisko}?"))
            dialogs.ShowMessage("Uložené.");
    }
}
