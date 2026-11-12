using MvvmToolkit.ViewModels;

namespace MvvmTesty;

public class ToolkitShellViewModelTests
{
    [Fact]
    public void UlozCommand_PrazdnyFormular_NedaSaVykonat()
    {
        var vm = new ShellViewModel(new FakeDialogs());

        Assert.False(vm.UlozCommand.CanExecute(null));
    }

    [Fact]
    public void UlozCommand_VyplnenyFormular_DaSaVykonat()
    {
        var vm = new ShellViewModel(new FakeDialogs()) { Meno = "Ján", Priezvisko = "Novák", Mail = "jan@uniza.sk" };

        Assert.True(vm.UlozCommand.CanExecute(null));
    }

    [Fact]
    public void ZmenaMailu_OznamiCanExecuteChanged()
    {
        // [NotifyCanExecuteChangedFor] – tlačidlo sa samo povolí / zakáže
        var vm = new ShellViewModel(new FakeDialogs());
        int pocet = 0;
        vm.UlozCommand.CanExecuteChanged += (_, _) => pocet++;

        vm.Mail = "jan@uniza.sk";

        Assert.Equal(1, pocet);
    }

    [Fact]
    public void Uloz_PoPotvrdeni_ZobraziUlozene()
    {
        var dialogs = new FakeDialogs();
        var vm = new ShellViewModel(dialogs) { Meno = "Ján", Priezvisko = "Novák", Mail = "jan@uniza.sk" };

        vm.UlozCommand.Execute(null);

        Assert.Equal("Uložené.", dialogs.Posledna);
    }
}
