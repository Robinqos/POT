using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MvvmSpolocne;
using MvvmToolkit.ViewModels;
using MvvmToolkit.Views;

namespace MvvmToolkit;

// composition root – jediné miesto, kde sa aplikácia skladá (Generic Host)
public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Services.AddSingleton<IDialogService, DialogService>();
        builder.Services.AddTransient<ShellViewModel>();
        builder.Services.AddTransient<ShellView>();
        _host = builder.Build();
    }

    protected override void OnStartup(StartupEventArgs e)
        => _host.Services.GetRequiredService<ShellView>().Show();

    protected override void OnExit(ExitEventArgs e) => _host.Dispose();
}
