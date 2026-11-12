using System.Windows;

namespace MvvmCaliburn;

public partial class App : Application
{
    public App()
    {
        // Bootstrapper sa zaregistruje na udalosť Startup a zobrazí ShellView
        var bootstrapper = new Bootstrapper();
    }
}
