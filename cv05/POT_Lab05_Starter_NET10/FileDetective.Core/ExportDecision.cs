namespace FileDetective.Core;

// Pripravená jednorazová brána medzi náhľadom a zápisom reportu.
public sealed class ExportDecision
{
    private readonly TaskCompletionSource<bool> _completion = new(TaskCreationOptions.RunContinuationsAsynchronously);

    // BEGIN U4
    public async Task<bool> WaitAsync(CancellationToken token)
    {
        // TODO U4a: registrácia rušenia žije cez await _completion.Task.
        using var registration = token.Register(
            () => _completion.TrySetCanceled(token));

        return await _completion.Task.ConfigureAwait(false);
    }



    // TODO U4b: prvé rozhodnutie vyhrá, opakované vráti false.
    public bool TryDecide(bool save) => _completion.TrySetResult(save);
    // END U4

    public bool TryFail(Exception error) => _completion.TrySetException(error);
}
