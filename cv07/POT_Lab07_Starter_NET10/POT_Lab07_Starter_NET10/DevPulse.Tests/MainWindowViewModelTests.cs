using DevPulse.Core.Models;
using DevPulse.Core.ViewModels;

namespace DevPulse.Tests;

public class MainWindowViewModelTests
{
    [Fact]
    public void Inicializacia_ObsahujePredvoleneZdrojeAUkazkoveUlohy()
    {
        var vm = new MainWindowViewModel(new FakeFeedService(), new FakeDialogService(), new FakeBrowserLauncher());

        Assert.NotEmpty(vm.Sources);
        Assert.NotEmpty(vm.Tasks);
    }

    // TODO [Úloha 4 / Fáza 4]: Doplňte xUnit testy (pokojne s pomocou AI agenta + mutačná skúška) pre:
    // 1. AddTaskCommand.CanExecute pri prázdnom/medzerovom a vyplnenom NewTaskTitle + udalosť CanExecuteChanged
    // 2. Pridanie novej úlohy na začiatok kolekcie Tasks a vyčistenie NewTaskTitle
    // 3. Potvrdenie vs. zamietnutie mazania splnených úloh cez FakeDialogService (NextConfirmationResult = false / true)
    // 4. Dokončenie a zrušenie asynchrónnej analýzy cez AnalyzeFeedCancelCommand (CancellationToken)
}
