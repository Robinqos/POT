namespace FileDetective.Core;

// Pripravená jednorazová brána medzi náhľadom a zápisom reportu.
public sealed class ExportDecision
{
    private readonly TaskCompletionSource<bool> _completion = new(TaskCreationOptions.RunContinuationsAsynchronously);

    // BEGIN U4
    public Task<bool> WaitAsync(CancellationToken token)
    {
        // TODO U4a: registrácia rušenia žije cez await _completion.Task.
        throw new NotImplementedException("TODO U4a: čakajte na rozhodnutie o reporte");
    }

    // TODO U4b: prvé rozhodnutie vyhrá, opakované vráti false.
    public bool TryDecide(bool save) => throw new NotImplementedException("TODO U4b: uložte rozhodnutie");
    // END U4

    public bool TryFail(Exception error) => _completion.TrySetException(error);
}
