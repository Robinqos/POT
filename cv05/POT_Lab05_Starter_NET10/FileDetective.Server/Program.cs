using FileDetective.Server;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://127.0.0.1:5085");
builder.Services.AddSingleton<ArchiveStore>();
var app = builder.Build();
app.MapGet("/health", () => Results.Ok("Archív je dostupný."));
app.MapPost("/api/reports", ArchiveEndpoints.Publish);
app.MapGet("/api/reports/{id:guid}", async (Guid id, ArchiveStore store, CancellationToken token) =>
{
    var report = await store.ReadAsync(id, token);
    return report == null ? Results.NotFound() : Results.Ok(report);
});
app.MapGet("/api/reports", (ArchiveStore store, CancellationToken token) => store.ListAsync(token));
app.Run();
