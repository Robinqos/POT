using System.Net;
using System.Net.Http.Json;
using FileDetective.Core;

namespace FileDetective.Tests;

public sealed class ArchiveTests
{
    private static ScanReport Report => new("DemoData", 11, 123, [], []);

    [Fact]
    public async Task U5_Publikuje_JSON_a_vrati_ID()
    {
        var id = Guid.NewGuid();
        using var http = new HttpClient(new ReplyHandler(async (request, ct) =>
        {
            Assert.Equal(HttpMethod.Post, request.Method); Assert.Equal("/api/reports", request.RequestUri!.AbsolutePath);
            var body = await request.Content!.ReadFromJsonAsync<ReportUpload>(ct);
            Assert.Equal(11, body!.ScannedFiles);
            return new HttpResponseMessage(HttpStatusCode.Created) { Content = JsonContent.Create(new ArchivedReport(id, DateTimeOffset.UtcNow, body)) };
        })) { BaseAddress = new Uri("http://archive/") };
        Assert.Equal(id, (await new ArchiveClient(http).PublishAsync(Report, false, default)).Id);
    }

    [Fact]
    public async Task U5_HTTP_chyba_nie_je_uspesna_publikacia()
    {
        using var http = new HttpClient(new ReplyHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable))))
            { BaseAddress = new Uri("http://archive/") };
        await Assert.ThrowsAsync<HttpRequestException>(() => new ArchiveClient(http).PublishAsync(Report, false, default));
    }

    [Fact]
    public async Task U5_Token_prerusi_prebiehajucu_poziadavku()
    {
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var source = new CancellationTokenSource();
        using var http = new HttpClient(new ReplyHandler(async (_, ct) =>
        { started.TrySetResult(); await Task.Delay(Timeout.Infinite, ct); return new HttpResponseMessage(HttpStatusCode.OK); }))
            { BaseAddress = new Uri("http://archive/") };
        Task<ArchivedReport>? work = null;
        try
        {
            work = new ArchiveClient(http).PublishAsync(Report, true, source.Token);
            await started.Task.WaitAsync(TimeSpan.FromSeconds(5)); source.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => work.WaitAsync(TimeSpan.FromSeconds(5)));
        }
        finally { source.Cancel(); await Cleanup.Observe(work); }
    }

    [Fact]
    public async Task Pripravene_WhenAll_zacne_obidve_poziadavky_pred_dokoncenim()
    {
        var allStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        int calls = 0;
        using var http = new HttpClient(new ReplyHandler(async (request, ct) =>
        {
            if (Interlocked.Increment(ref calls) == 2) allStarted.TrySetResult();
            await release.Task.WaitAsync(ct);
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = request.RequestUri!.AbsolutePath == "/health"
                ? JsonContent.Create("ready") : JsonContent.Create(Array.Empty<ArchivedReport>()) };
        })) { BaseAddress = new Uri("http://archive/") };
        var work = new ArchiveClient(http).CheckAsync(default);
        try { await allStarted.Task.WaitAsync(TimeSpan.FromSeconds(5)); release.TrySetResult(); Assert.Empty(await work); }
        finally { release.TrySetResult(); await Cleanup.Observe(work); }
    }

    private sealed class ReplyHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> reply) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token) => reply(request, token);
    }
}
