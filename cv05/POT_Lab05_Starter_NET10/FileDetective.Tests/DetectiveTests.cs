using System.Security.Cryptography;
using System.Text;
using FileDetective.Core;

namespace FileDetective.Tests;

public sealed class PreparedReaderTests
{
    [Fact]
    public async Task Pripravene_Citanie_Precita_skutocne_bajty()
    {
        using var data = new TestFolder();
        string path = data.Put("text.txt", "Príliš žltý kôň");
        Assert.Equal(await File.ReadAllBytesAsync(path), await new FileReader().ReadAsync(path, default));
    }

    [Fact]
    public async Task Pripravene_Citanie_Prazdny_subor_je_platny()
    {
        using var data = new TestFolder();
        Assert.Empty(await new FileReader().ReadAsync(data.Put("empty.txt", ""), default));
    }

    [Fact]
    public async Task Pripravene_Citanie_Chybajuci_subor_nie_je_prazdny_uspech()
    {
        using var data = new TestFolder();
        await Assert.ThrowsAsync<FileNotFoundException>(() => new FileReader().ReadAsync(Path.Combine(data.Root, "missing"), default));
    }

    [Fact]
    public async Task Pripravene_Citanie_Respektuje_token()
    {
        using var data = new TestFolder();
        using var source = new CancellationTokenSource(); source.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            new FileReader().ReadAsync(data.Put("file.txt", "abc"), source.Token));
    }

    [Fact]
    public async Task Pripravene_Citanie_Nacitanie_ma_limit_pamate()
    {
        using var data = new TestFolder(); string path = Path.Combine(data.Root, "large.bin");
        using (var file = File.Create(path)) file.SetLength(FileReader.MaxFileBytes + 1L);
        await Assert.ThrowsAsync<IOException>(() => new FileReader().ReadAsync(path, default));
    }
}

public sealed class ScannerTests
{
    [Fact]
    public void U2a_Odtlacok_pokrýva_cely_obsah()
    {
        byte[] bytes = new byte[Fingerprint.ChunkSize * 3 + 1];
        new Random(10).NextBytes(bytes);
        Assert.Equal(Convert.ToHexString(SHA256.HashData(bytes)), Fingerprint.Compute(bytes, default));
    }

    [Fact]
    public void U2a_Zrusenie_zastavi_beziacu_slucku()
    {
        using var source = new CancellationTokenSource(); int chunks = 0;
        byte[] bytes = new byte[Fingerprint.ChunkSize * 8];
        Assert.ThrowsAny<OperationCanceledException>(() => Fingerprint.Compute(bytes, source.Token,
            _ => { if (++chunks == 2) source.Cancel(); }));
        Assert.Equal(2, chunks);
    }

    [Fact]
    public async Task U2b_Premenovanie_nemeni_odtlacok()
    {
        using var data = new TestFolder();
        var scanner = new DuplicateScanner(new FileReader());
        var a = await scanner.HashFileAsync(data.Put("prvy.txt", "rovnaky obsah"), default);
        var b = await scanner.HashFileAsync(data.Put("iny-nazov.txt", "rovnaky obsah"), default);
        Assert.Equal(a.Hash, b.Hash); Assert.Equal(a.Length, b.Length);
    }

    [Fact]
    public async Task U2b_Zmena_za_prvym_blokom_sa_nestrati()
    {
        using var data = new TestFolder();
        byte[] a = new byte[Fingerprint.ChunkSize * 2]; byte[] b = (byte[])a.Clone(); b[^1] = 1;
        var scanner = new DuplicateScanner(new FileReader());
        var fa = await scanner.HashFileAsync(data.Put("a.bin", a), default);
        var fb = await scanner.HashFileAsync(data.Put("b.bin", b), default);
        Assert.NotEqual(fa.Hash, fb.Hash);
    }

    [Fact]
    public async Task U2b_Demo_ma_tri_skupiny_a_styri_nadbytocne_kopie()
    {
        var progress = new List<ScanProgress>();
        var report = await new DuplicateScanner(new FileReader()).ScanAsync(
            Path.Combine(AppContext.BaseDirectory, "DemoData"), new InlineProgress<ScanProgress>(progress.Add), default);
        Assert.Equal(11, report.ScannedFiles); Assert.Equal(3, report.Groups.Count);
        Assert.Equal(4, report.ExtraCopies); Assert.Empty(report.Issues);
        Assert.DoesNotContain(report.Groups.SelectMany(g => g.Paths), p => Path.GetFileName(p) == "plan.txt");
        Assert.Equal(11, progress[^1].Done);
    }

    [Fact]
    public async Task U2b_Rovnaky_nazov_a_velkost_nestacia()
    {
        using var data = new TestFolder();
        data.Put("A/plan.txt", "sever"); data.Put("B/plan.txt", "juh!!");
        var report = await new DuplicateScanner(new FileReader()).ScanAsync(data.Root, null, default);
        Assert.Empty(report.Groups); Assert.Equal(2, report.ScannedFiles);
    }

    [Fact]
    public async Task U2b_Zrusenie_pocas_citania_sa_nepohlti()
    {
        using var source = new CancellationTokenSource();
        var reader = new ControlledReader(); Task<FingerprintEntry>? work = null;
        try
        {
            work = new DuplicateScanner(reader).HashFileAsync("file", source.Token);
            await reader.Started.Task.WaitAsync(TimeSpan.FromSeconds(5)); source.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => work.WaitAsync(TimeSpan.FromSeconds(5)));
        }
        finally { reader.Complete(); await Cleanup.Observe(work); }
    }

    [Fact]
    public async Task U2b_Chyba_jedneho_suboru_sa_zapise_do_reportu()
    {
        using var data = new TestFolder(); data.Put("one.txt", "x");
        var report = await new DuplicateScanner(new BrokenReader()).ScanAsync(data.Root, null, default);
        Assert.Equal(0, report.ScannedFiles); Assert.Single(report.Issues);
    }
}

public sealed class BudgetTests
{
    [Fact]
    public async Task U3_Vrati_dokonceny_report()
    {
        var scanner = new ControlledScanner(); Task<ScanReport>? work = null;
        try
        {
            work = new BudgetScanner(scanner).RunAsync("root", default, default);
            await scanner.Started.Task.WaitAsync(TimeSpan.FromSeconds(5)); scanner.Complete();
            Assert.Same(scanner.Report, await work.WaitAsync(TimeSpan.FromSeconds(5)));
        }
        finally { scanner.Complete(); await Cleanup.Observe(work); }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task U3_Rozlisi_zrusenie_a_timeout(bool timeout)
    {
        using var user = new CancellationTokenSource(); using var deadline = new CancellationTokenSource();
        var scanner = new ControlledScanner(); Task<ScanReport>? work = null;
        try
        {
            work = new BudgetScanner(scanner).RunAsync("root", user.Token, deadline.Token);
            await scanner.Started.Task.WaitAsync(TimeSpan.FromSeconds(5));
            if (timeout)
            {
                deadline.Cancel();
                await Assert.ThrowsAsync<TimeoutException>(() => work.WaitAsync(TimeSpan.FromSeconds(5)));
                Assert.True(work.IsFaulted);
            }
            else
            {
                user.Cancel();
                var ex = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => work.WaitAsync(TimeSpan.FromSeconds(5)));
                Assert.Equal(user.Token, ex.CancellationToken); Assert.True(work.IsCanceled);
            }
            Assert.True(scanner.Ended);
        }
        finally { scanner.Complete(); await Cleanup.Observe(work); }
    }

    [Fact]
    public async Task U3_Pri_oboch_signaloch_ma_prednost_pouzivatel()
    {
        using var user = new CancellationTokenSource(); user.Cancel();
        using var deadline = new CancellationTokenSource(); deadline.Cancel();
        var scanner = new ControlledScanner(); Task<ScanReport>? work = null;
        try
        {
            var ex = await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                (work = new BudgetScanner(scanner).RunAsync("root", user.Token, deadline.Token)).WaitAsync(TimeSpan.FromSeconds(5)));
            Assert.Equal(user.Token, ex.CancellationToken); Assert.False(scanner.Started.Task.IsCompleted);
        }
        finally { scanner.Complete(); await Cleanup.Observe(work); }
    }
}

public sealed class DecisionTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task U4_Zachova_rozhodnutie_a_ignoruje_opakovanie(bool save)
    {
        var decision = new ExportDecision(); var pending = decision.WaitAsync(default);
        Assert.False(pending.IsCompleted); Assert.True(decision.TryDecide(save));
        Assert.False(decision.TryDecide(!save));
        Assert.Equal(save, await pending.WaitAsync(TimeSpan.FromSeconds(5)));
    }

    [Fact]
    public async Task U4_Skore_rozhodnutie_sa_nestrati()
    {
        var decision = new ExportDecision(); Assert.True(decision.TryDecide(false));
        Assert.False(await decision.WaitAsync(default).WaitAsync(TimeSpan.FromSeconds(5)));
    }

    [Fact]
    public async Task U4_Zrusenie_zatvori_tuto_branu()
    {
        using var source = new CancellationTokenSource();
        var decision = new ExportDecision(); var pending = decision.WaitAsync(source.Token); source.Cancel();
        var ex = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pending.WaitAsync(TimeSpan.FromSeconds(5)));
        Assert.Equal(source.Token, ex.CancellationToken); Assert.False(decision.TryDecide(true));
    }
}

public sealed class PreparedTests
{
    [Fact]
    public async Task Pripravene_Prehliadka_vidi_aj_podpriecinky()
    {
        using var data = new TestFolder(); data.Put("a/one.txt", "x"); data.Put("b/two.txt", "yy");
        var items = await FileCatalog.ListAsync(data.Root, default);
        Assert.Equal(2, items.Count); Assert.Equal(3, items.Sum(i => i.Length));
    }

    [Fact]
    public void Pripravene_Prazdny_obsah_ma_platny_odtlacok() =>
        Assert.Equal(Convert.ToHexString(SHA256.HashData([])), Fingerprint.Compute([], default));

    [Fact]
    public async Task Pripravene_Report_je_skutocny_HTML_a_neprepise_stary_subor()
    {
        using var data = new TestFolder(); string old = data.Put("existing.html", "povodny report");
        var report = new ScanReport(data.Root, 0, 0, [], []);
        string path = await ReportExporter.SaveAsync(report, data.Root, default);
        Assert.Contains("<!doctype html>", await File.ReadAllTextAsync(path));
        Assert.Equal("povodny report", await File.ReadAllTextAsync(old));
        Assert.NotEqual(old, path);
    }
}
