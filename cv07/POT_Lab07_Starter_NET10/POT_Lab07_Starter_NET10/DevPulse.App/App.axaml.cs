using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using DevPulse.App.Services;
using DevPulse.App.Views;
using DevPulse.Core.Services;
using DevPulse.Core.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DevPulse.App;

public partial class App : Application
{
    private IHost? _host;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // INŠPEKCIA [Úloha 2 & 3 – Composition Root]: Jediné miesto v aplikácii, kde sa konkrétne implementácie služieb
        // (HttpFeedService, AvaloniaDialogService, SystemBrowserLauncher) registrujú do Dependency Injection kontajnera.
        var builder = Host.CreateApplicationBuilder();
        builder.Services.AddSingleton<IFeedService, HttpFeedService>();
        builder.Services.AddSingleton<IDialogService, AvaloniaDialogService>();
        builder.Services.AddSingleton<IBrowserLauncher, SystemBrowserLauncher>();
        builder.Services.AddTransient<MainWindowViewModel>();
        _host = builder.Build();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = _host.Services.GetRequiredService<MainWindowViewModel>()
            };

            desktop.Exit += (_, _) => _host.Dispose();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
