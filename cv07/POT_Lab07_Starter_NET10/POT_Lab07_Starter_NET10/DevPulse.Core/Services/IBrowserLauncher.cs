using System.Diagnostics;

namespace DevPulse.Core.Services;

/// <summary>
/// Abstrakcia pre otváranie URL odkazov v systémovom prehliadači.
/// Vďaka rozhraniu pri unit testoch neotvárame skutočný prehliadač.
/// </summary>
public interface IBrowserLauncher
{
    void OpenUrl(string url);
}

public sealed class SystemBrowserLauncher : IBrowserLauncher
{
    public static bool IsSafeWebUrl(string? url, out Uri? validatedUri)
    {
        validatedUri = null;
        if (string.IsNullOrWhiteSpace(url))
            return false;

        if (!Uri.TryCreate(url.Trim(), UriKind.Absolute, out var candidate))
            return false;

        if (candidate.Scheme != Uri.UriSchemeHttp && candidate.Scheme != Uri.UriSchemeHttps)
            return false;

        validatedUri = candidate;
        return true;
    }

    public void OpenUrl(string url)
    {
        if (!IsSafeWebUrl(url, out var safeUri) || safeUri is null)
            return;

        // Multiplatformové otvorenie overeného http/https odkazu cez predvolený prehliadač
        Process.Start(new ProcessStartInfo
        {
            FileName = safeUri.AbsoluteUri,
            UseShellExecute = true
        });
    }
}
