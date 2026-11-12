using System.Windows;
using Caliburn.Micro;
using MvvmCaliburn.ViewModels;
using MvvmSpolocne;

namespace MvvmCaliburn;

// slajdy 15 a 19 – Bootstrapper so SimpleContainer
public class Bootstrapper : BootstrapperBase
{
    private readonly SimpleContainer _container = new();

    public Bootstrapper() => Initialize();

    protected override void Configure()
    {
        _container.Singleton<IWindowManager, WindowManager>();
        _container.Singleton<IDialogService, DialogService>();
        _container.PerRequest<ShellViewModel>();
    }

    protected override object GetInstance(Type service, string key) => _container.GetInstance(service, key);
    protected override IEnumerable<object> GetAllInstances(Type service) => _container.GetAllInstances(service);
    protected override void BuildUp(object instance) => _container.BuildUp(instance);

    protected override async void OnStartup(object sender, StartupEventArgs e)
        => await DisplayRootViewForAsync<ShellViewModel>();
}
