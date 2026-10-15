using System.Net;
using NeonovyKurier.Core;

namespace NeonovyKurier.Tests;

public sealed class DownloadTests
{
    private static GameAsset Asset(string id = "a", bool required = false) => new(id, id + ".txt", "/" + id, required);
    private static HttpClient Client(ControlledHandler handler) => new(handler) { BaseAddress = new Uri("http://localhost/") };
    private static string NewDirectory() => Path.Combine(Path.GetTempPath(), "NeonovyKurierTests", Guid.NewGuid().ToString("N"));

    [Fact, Trait("Unit", "Pripravene")]
    public void Pripravene_PathFor_JoinsDirectoryAndName() => Assert.Equal(Path.Combine("pack", "a.txt"), PackageFiles.PathFor("pack", Asset()));

    [Fact, Trait("Unit", "Pripravene")]
    public void Pripravene_PathFor_RejectsNestedName() => Assert.Throws<ArgumentException>(() => PackageFiles.PathFor("pack", Asset() with { FileName = "../a.txt" }));

    [Fact, Trait("Unit", "U2")]
    public async Task U2_Download_ReturnsEntireContentAndIdentity()
    {
        using var handler = new ControlledHandler(); using var client = Client(handler);
        handler.Complete("a", "Level\nŽilina");
        DownloadResult result = await new DownloadService(client).DownloadAssetAsync(Asset());
        Assert.Equal(new DownloadResult(Asset(), "Level\nŽilina"), result);
    }

    [Fact, Trait("Unit", "U2")]
    public async Task U2_Download_IsPendingUntilHttpResponse()
    {
        using var handler = new ControlledHandler(); using var client = Client(handler);
        Task<DownloadResult> work = new DownloadService(client).DownloadAssetAsync(Asset());
        await handler.StartedAsync("a"); Assert.False(work.IsCompleted);
        handler.Complete("a", "OK"); Assert.Equal("OK", (await work).Content);
    }

    [Fact, Trait("Unit", "U2")]
    public async Task U2_Download_ReportsStartThenDone()
    {
        using var handler = new ControlledHandler(); using var client = Client(handler);
        handler.Complete("a", "OK"); var progress = new RecordingProgress();
        await new DownloadService(client, progress).DownloadAssetAsync(Asset());
        Assert.Equal(new[] { "START", "DONE" }, progress.Events.Select(e => e.Stage));
    }

    [Fact, Trait("Unit", "U2")]
    public async Task U2_Download_PropagatesHttpFailure()
    {
        using var handler = new ControlledHandler(); using var client = Client(handler);
        handler.Complete("a", "Výpadok", HttpStatusCode.ServiceUnavailable);
        await Assert.ThrowsAsync<HttpRequestException>(() => new DownloadService(client).DownloadAssetAsync(Asset()));
    }

    [Fact, Trait("Unit", "U2")]
    public async Task U2_Download_ForwardsCancellationToHttp()
    {
        using var handler = new ControlledHandler(); using var client = Client(handler); using var cancel = new CancellationTokenSource();
        Task<DownloadResult> work = new DownloadService(client).DownloadAssetAsync(Asset(), cancel.Token);
        await handler.StartedAsync("a"); cancel.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => work.WaitAsync(TimeSpan.FromSeconds(5)));
        Assert.True(work.IsCanceled);
    }

    [Fact, Trait("Unit", "U2")]
    public async Task U2_Download_RejectsNull()
    {
        using var handler = new ControlledHandler(); using var client = Client(handler);
        await Assert.ThrowsAsync<ArgumentNullException>(() => new DownloadService(client).DownloadAssetAsync(null!));
    }

    [Fact, Trait("Unit", "U3a")]
    public async Task U3a_Sequential_StartsNextOnlyAfterPreviousFinishes()
    {
        using var handler = new ControlledHandler(); using var client = Client(handler); using var cancel = new CancellationTokenSource(5000);
        Task<DownloadResult[]> work = new DownloadService(client).DownloadSequentialAsync([Asset("a"), Asset("b")], cancel.Token);
        await handler.StartedAsync("a"); Assert.Equal(new[] { "a" }, handler.Requests);
        handler.Complete("a", "A"); await handler.StartedAsync("b"); handler.Complete("b", "B");
        Assert.Equal(new[] { "a", "b" }, (await work).Select(r => r.Asset.Id));
    }

    [Fact, Trait("Unit", "U3a")]
    public async Task U3a_Sequential_EmptyInput()
    {
        using var handler = new ControlledHandler(); using var client = Client(handler);
        Assert.Empty(await new DownloadService(client).DownloadSequentialAsync([])); Assert.Empty(handler.Requests);
    }

    [Fact, Trait("Unit", "U3a")]
    public async Task U3a_Sequential_ErrorStopsFurtherRequests()
    {
        using var handler = new ControlledHandler(); using var client = Client(handler);
        handler.Complete("a", "Výpadok", HttpStatusCode.ServiceUnavailable);
        await Assert.ThrowsAsync<HttpRequestException>(() => new DownloadService(client).DownloadSequentialAsync([Asset("a"), Asset("b")]));
        Assert.Equal(new[] { "a" }, handler.Requests);
    }

    [Fact, Trait("Unit", "U3b")]
    public async Task U3b_Concurrent_StartsAllBeforeAnyFinishesAndPreservesOrder()
    {
        using var handler = new ControlledHandler(); using var client = Client(handler); using var cancel = new CancellationTokenSource(5000);
        Task<DownloadResult[]> work = new DownloadService(client).DownloadConcurrentAsync([Asset("a"), Asset("b"), Asset("c")], cancel.Token);
        await Task.WhenAll(handler.StartedAsync("a"), handler.StartedAsync("b"), handler.StartedAsync("c"));
        handler.Complete("c", "C"); handler.Complete("b", "B"); Assert.False(work.IsCompleted);
        handler.Complete("a", "A");
        Assert.Equal(new[] { "a", "b", "c" }, (await work).Select(r => r.Asset.Id));
    }

    [Fact, Trait("Unit", "U3b")]
    public async Task U3b_Concurrent_EmptyInput()
    {
        using var handler = new ControlledHandler(); using var client = Client(handler);
        Assert.Empty(await new DownloadService(client).DownloadConcurrentAsync([])); Assert.Empty(handler.Requests);
    }

    [Fact, Trait("Unit", "U3b")]
    public async Task U3b_Concurrent_FailureStillWaitsForOtherTask()
    {
        using var handler = new ControlledHandler(); using var client = Client(handler); using var cancel = new CancellationTokenSource(5000);
        Task<DownloadResult[]> work = new DownloadService(client).DownloadConcurrentAsync([Asset("a"), Asset("b")], cancel.Token);
        await Task.WhenAll(handler.StartedAsync("a"), handler.StartedAsync("b"));
        handler.Complete("a", "Výpadok", HttpStatusCode.ServiceUnavailable); Assert.False(work.IsCompleted);
        handler.Complete("b", "B"); await Assert.ThrowsAsync<HttpRequestException>(() => work);
    }

    [Fact, Trait("Unit", "U4")]
    public async Task U4_Save_WritesAllFilesWithUtf8Content()
    {
        string directory = NewDirectory();
        try
        {
            using var handler = new ControlledHandler(); using var client = Client(handler);
            await new DownloadService(client).SavePackAsync(directory, [new(Asset("a"), "Žemľa"), new(Asset("b"), "B")]);
            Assert.Equal("Žemľa", await File.ReadAllTextAsync(Path.Combine(directory, "a.txt")));
            Assert.Equal("B", await File.ReadAllTextAsync(Path.Combine(directory, "b.txt")));
        }
        finally { if (Directory.Exists(directory)) Directory.Delete(directory, true); }
    }

    [Fact, Trait("Unit", "U4")]
    public async Task U4_Save_OverwritesOldContent()
    {
        string directory = NewDirectory(); Directory.CreateDirectory(directory);
        try
        {
            await File.WriteAllTextAsync(Path.Combine(directory, "a.txt"), "OLD");
            using var handler = new ControlledHandler(); using var client = Client(handler);
            await new DownloadService(client).SavePackAsync(directory, [new(Asset(), "NEW")]);
            Assert.Equal("NEW", await File.ReadAllTextAsync(Path.Combine(directory, "a.txt")));
        }
        finally { Directory.Delete(directory, true); }
    }

    [Fact, Trait("Unit", "U4")]
    public async Task U4_Save_CanceledBeforeStartCreatesNoDirectory()
    {
        string directory = NewDirectory(); using var handler = new ControlledHandler(); using var client = Client(handler);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => new DownloadService(client).SavePackAsync(directory, [], new CancellationToken(true)));
        Assert.False(Directory.Exists(directory));
    }

    [Fact, Trait("Unit", "U4")]
    public async Task U4_Save_EmptyPathRejected()
    {
        using var handler = new ControlledHandler(); using var client = Client(handler);
        await Assert.ThrowsAsync<ArgumentException>(() => new DownloadService(client).SavePackAsync("", []));
    }

    private sealed class RecordingProgress : IProgress<DownloadProgress>
    {
        internal List<DownloadProgress> Events { get; } = [];
        public void Report(DownloadProgress value) => Events.Add(value);
    }
}
