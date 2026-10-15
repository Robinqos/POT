using System.Net;
using System.Collections.Concurrent;

namespace NeonovyKurier.Tests;

// Riadené dokončovanie HTTP odpovedí. Testy súbežnosti nemerajú milisekundy.
internal sealed class ControlledHandler : HttpMessageHandler
{
    private readonly ConcurrentDictionary<string, TaskCompletionSource> _started = new();
    private readonly ConcurrentDictionary<string, TaskCompletionSource<HttpResponseMessage>> _responses = new();
    private readonly ConcurrentQueue<string> _requests = new();
    internal string[] Requests => _requests.ToArray();

    private TaskCompletionSource StartedGate(string id) => _started.GetOrAdd(id,
        _ => new(TaskCreationOptions.RunContinuationsAsynchronously));
    private TaskCompletionSource<HttpResponseMessage> ResponseGate(string id) => _responses.GetOrAdd(id,
        _ => new(TaskCreationOptions.RunContinuationsAsynchronously));

    internal Task StartedAsync(string id) => StartedGate(id).Task.WaitAsync(TimeSpan.FromSeconds(5));
    internal void Complete(string id, string text, HttpStatusCode status = HttpStatusCode.OK)
        => ResponseGate(id).SetResult(new HttpResponseMessage(status) { Content = new StringContent(text) });

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        string id = request.RequestUri!.AbsolutePath.Trim('/');
        _requests.Enqueue(id);
        StartedGate(id).TrySetResult();
        return await ResponseGate(id).Task.WaitAsync(cancellationToken);
    }
}
