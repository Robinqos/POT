using MvvmCaliburn.ViewModels;

namespace MvvmTesty;

// ViewModel sa testuje bez okna – „ViewModel brať ako reálne UI“ (zásady MVVM)
public class CaliburnShellViewModelTests
{
    [Fact]
    public void CanCmdSave_PrazdnyFormular_JeFalse()
    {
        var vm = new ShellViewModel(new FakeDialogs());

        Assert.False(vm.CanCmdSave);
    }

    [Fact]
    public void CanCmdSave_MailBezZavinaca_JeFalse()
    {
        var vm = new ShellViewModel(new FakeDialogs()) { TxtMeno = "Ján", TxtPrie = "Novák", TxtMail = "jan.uniza.sk" };

        Assert.False(vm.CanCmdSave);
    }

    [Fact]
    public void ZmenaMena_OznamiZmenuCanCmdSave()
    {
        var vm = new ShellViewModel(new FakeDialogs());
        var zmenene = new List<string?>();
        vm.PropertyChanged += (_, e) => zmenene.Add(e.PropertyName);

        vm.TxtMeno = "Ján";

        Assert.Contains(nameof(ShellViewModel.TxtMeno), zmenene);
        Assert.Contains(nameof(ShellViewModel.CanCmdSave), zmenene);
    }

    [Fact]
    public void CmdSave_PoPotvrdeni_ZobraziUlozene()
    {
        var dialogs = new FakeDialogs();
        var vm = new ShellViewModel(dialogs) { TxtMeno = "Ján", TxtPrie = "Novák", TxtMail = "jan@uniza.sk" };

        vm.CmdSave();

        Assert.Equal("Uložené.", dialogs.Posledna);
    }

    [Fact]
    public void CmdSave_PoOdmietnuti_NicNezobrazi()
    {
        var dialogs = new FakeDialogs { Odpoved = false };
        var vm = new ShellViewModel(dialogs) { TxtMeno = "Ján", TxtPrie = "Novák", TxtMail = "jan@uniza.sk" };

        vm.CmdSave();

        Assert.Equal(1, dialogs.PocetOtazok);
        Assert.Null(dialogs.Posledna);
    }
}
