using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace NeonovyKurier.Cli.Infrastructure;

// Hotová infraštruktúra: malé textové súbory cez skutočné HTTP, riadená latencia.
internal sealed class DemoServer(WebApplication app, Uri address) : IAsyncDisposable
{
    internal Uri BaseAddress { get; } = address;

    internal static async Task<DemoServer> StartAsync()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = [], EnvironmentName = "Development", ContentRootPath = AppContext.BaseDirectory
        });
        builder.Logging.ClearProviders();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        var app = builder.Build();
        var routes = new (string Route, string File, int Delay)[]
        {
            ("/level", "level.json", 1200),
            ("/theme", "theme.json", 700),
            ("/tips", "tips.txt", 900)
        };
        foreach (var route in routes)
            app.MapGet(route.Route, async (HttpContext context) =>
            {
                await Task.Delay(route.Delay, context.RequestAborted);
                string content = await File.ReadAllTextAsync(
                    Path.Combine(MissionContent.SourceDirectory, route.File), context.RequestAborted);
                return Results.Text(content, "text/plain; charset=utf-8");
            });
        foreach (var route in routes)
            app.MapGet("/slow" + route.Route, async (HttpContext context) =>
            {
                await Task.Delay(5000, context.RequestAborted);
                return Results.Text(await File.ReadAllTextAsync(
                    Path.Combine(MissionContent.SourceDirectory, route.File), context.RequestAborted));
            });
        app.MapGet("/failure", () => Results.Text("Distribučný server má výpadok.", statusCode: 503));
        app.MapGet("/slow", async (HttpContext context) =>
        {
            await Task.Delay(5000, context.RequestAborted);
            return Results.Text("Pomalá odpoveď");
        });
        try
        {
            await app.StartAsync();
            var feature = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!;
            return new DemoServer(app, new Uri(feature.Addresses.Single()));
        }
        catch { await app.DisposeAsync(); throw; }
    }

    public async ValueTask DisposeAsync()
    {
        await app.StopAsync();
        await app.DisposeAsync();
    }
}
