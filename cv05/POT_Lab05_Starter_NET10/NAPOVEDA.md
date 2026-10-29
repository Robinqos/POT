# Nápoveda pre audit súborov

Použite iba časť k aktuálnej karte. Každá časť má najprv postup bez kódu. Úplný vzor je zbalený; otvorte ho až po vlastnom pokuse alebo pri porovnaní riešenia. Vzory patria do existujúcich signatúr a pripraveného kódu.

## U1 Hotový náhľad z Lab04

```csharp
public async Task<byte[]> ReadAsync(string path, CancellationToken token)
{
    return await ReadBoundedAsync(path, token);
}
```

Tento vzor už je súčasťou startera; znova ho nedopĺňate. ReadBoundedAsync už používa skutočné FileStream.ReadAsync a obmedzuje jeden súbor na 32 MiB. Nepridávajte Task.Run okolo asynchrónneho čítania. V metóde je pri chybe zlyhaný Task; obsluha v UI zachytí chybu pri await. Samotné async ešte nevytvára pracovné vlákno.

## U2a Kontrolný bod CPU slučky

**Postup:** Kam v slučke patrí kontrola, aby sa po zrušení nespracoval ďalší blok? Test zruší token po druhom bloku a očakáva presne dva spracované bloky.

<details>
<summary>Úplný vzor, otvorte až po vlastnom pokuse</summary>

```csharp
token.ThrowIfCancellationRequested();
```

</details>

Kontrola len pred slučkou neukončí rozbehnutú prácu. Kontrola až pred návratom môže vyhodiť správnu výnimku príliš neskoro, keď sa už vypočítal celý obsah. Test preto sleduje počet spracovaných blokov, nielen typ výnimky.

## U2b Čítanie a výpočet

**Postup:** 1) dočkajte bajty z čítačky, 2) CPU výpočet spustite cez Task.Run a dočkajte, 3) vráťte nový záznam. Token odovzdajte čítaniu, výpočtu aj plánovaniu Task.Run.

<details>
<summary>Úplný vzor, otvorte až po vlastnom pokuse</summary>

```csharp
public async Task<FingerprintEntry> HashFileAsync(string path, CancellationToken token)
{
    byte[] bytes = await reader.ReadAsync(path, token);
    string hash = await Task.Run(
        () => Fingerprint.Compute(bytes, token), token);
    return new FingerprintEntry(path, bytes.LongLength, hash);
}
```

</details>

Čítanie čaká na disk. Fingerprint.Compute je synchrónna CPU práca nad načítaným poľom, preto ju v desktopovej aplikácii presúvame mimo UI. Token plánovania nezastaví už bežiaci delegát; ten spolupracuje v U2a. Task.FromResult(Fingerprint.Compute(...)) by najprv vypočítal výsledok na volajúcom vlákne.

## U3 Prepojené zdroje zrušenia

**Postup:** Linked zdroj vytvorte cez using, skontrolujte jeho token a sken dočkajte v try. V catch použite filter when a najprv testujte userToken. Výnimku s konkrétnym tokenom vytvoríte cez new OperationCanceledException(token).

<details>
<summary>Úplný vzor, otvorte až po vlastnom pokuse</summary>

```csharp
public async Task<ScanReport> RunAsync(string root, CancellationToken userToken,
    CancellationToken deadlineToken, IProgress<ScanProgress>? progress = null)
{
    using var linked = CancellationTokenSource.CreateLinkedTokenSource(
        userToken, deadlineToken);
    try
    {
        linked.Token.ThrowIfCancellationRequested();
        return await scanner.ScanAsync(root, progress, linked.Token);
    }
    catch (OperationCanceledException) when (userToken.IsCancellationRequested)
    {
        throw new OperationCanceledException(userToken);
    }
    catch (OperationCanceledException) when (deadlineToken.IsCancellationRequested)
    {
        throw new TimeoutException("Vypršal limit auditu.");
    }
}
```

</details>

Using sa skončí až po await. Ak iba vrátite rozbehnutý Task bez await, linked zdroj môže zaniknúť predčasne. Pri oboch aktívnych tokenoch v čase catch má podľa dohody prednosť používateľ. Časový limit je zlyhanie, ručné zrušenie je zrušený Task. Nejde o pevný časový limit pre kód, ktorý token ignoruje.

## U4 Rozhodnutie o reporte

**Postup:** Token.Register prijme callback, ktorý zruší pripravený TCS. Výsledok registrácie držte v using počas await na _completion.Task. TryDecide je jeden riadok s TrySetResult.

<details>
<summary>Úplný vzor, otvorte až po vlastnom pokuse</summary>

```csharp
public async Task<bool> WaitAsync(CancellationToken token)
{
    using var registration = token.Register(
        () => _completion.TrySetCanceled(token));
    return await _completion.Task;
}

public bool TryDecide(bool save) => _completion.TrySetResult(save);
```

</details>

True znamená uložiť, false znamená neukladať. TaskCompletionSource s RunContinuationsAsynchronously je pripravený. Brána je jednorazová; nové kliknutie Pripraviť uloženie vytvorí novú. Registráciu neukončite skôr než čakanie. TrySet metódy dovolia ignorovať opakovaný signál bez výnimky. Zrušenie zatvorí túto bránu, nie iba jedno čakanie na ňu.

## U5a HTTP klient

**Postup:** Odošlite POST s telom a tokenom, overte stav cez EnsureSuccessStatusCode a dočkajte ReadFromJsonAsync s tokenom. Táto metóda môže vrátiť null; projekt berie varovania ako chyby, preto bez ?? throw build skončí chybou CS8603.

<details>
<summary>Úplný vzor, otvorte až po vlastnom pokuse</summary>

```csharp
public async Task<ArchivedReport> PublishAsync(ScanReport report, bool slow, CancellationToken token)
{
    using var response = await http.PostAsJsonAsync(
        "api/reports" + (slow ? "?delayMs=3000" : ""),
        ReportUpload.From(report), token);
    response.EnsureSuccessStatusCode();
    return await response.Content.ReadFromJsonAsync<ArchivedReport>(token)
        ?? throw new InvalidDataException("Archív vrátil prázdnu odpoveď.");
}
```

</details>

Token musí dostať aj čítanie odpovede. HTTP 404 alebo 503 nie je platný ArchivedReport. Pripravený klient sa používa opakovane; nevytvárajte nový HttpClient na každú požiadavku.

## U5b Serverový endpoint

**Postup:** Dočkajte SaveAsync s tokenom endpointu a vráťte Results.Created s cestou a uloženým objektom. Automatické testy server nespúšťajú; okno 5 vypíše skutočný stav odpovede aj hlavičku Location.

<details>
<summary>Úplný vzor, otvorte až po vlastnom pokuse</summary>

```csharp
private static async Task<IResult> SaveValidAsync(ReportUpload report, int delayMs,
    ArchiveStore store, CancellationToken token)
{
    var saved = await store.SaveAsync(report, delayMs, token);
    return Results.Created($"/api/reports/{saved.Id}", saved);
}
```

</details>

Pripravený handler Publish dostáva CancellationToken z RequestAborted. Uloženie potrebuje tento token aj počas Task.Delay a File.WriteAllTextAsync. Task.Run tu nepridávajte; ide o asynchrónne I/O na serveri. Lokálna CPU fáza v U2 má iný dôvod na presun práce.

## Doplnkové krátke experimenty

CheckAsync začína overenie zdravia a načítanie zoznamu pred await Task.WhenAll. Keď presuniete prvý await pred začatie druhého volania, stratíte súbeh. Dodaný test Pripravene_WhenAll_zacne_obidve_poziadavky_pred_dokoncenim túto zmenu odmietne.

Server vracia IAsyncEnumerable a klient ho spotrebúva cez await foreach. Je to asynchrónne získavanie položiek; nezaručuje pevnú prestávku medzi položkami a transport môže dáta bufferovať. EnumeratorCancellation prepája token spotrebiteľa s telom iterátora. Uvoľnenie enumerátora spravuje await foreach.

V hotovej validácii endpointu nájdite Task.FromResult. V BrokenReader v testoch nájdite Task.FromException. Porovnajte okamžité vrátenie Tasku s async metódou. Samostatná metóda bez výsledku môže pri okamžitom úspechu vrátiť Task.CompletedTask. Tieto formy nepresúvajú výpočet mimo volajúceho vlákna.

Konštruktor nemôže byť async. Ak by klient vyžadoval povinnú asynchrónnu inicializáciu, navrhnite CreateAsync, ktorá vráti až pripravený objekt. Nezačínajte z konštruktora nezapamätaný Task.

Pokus s blokovaním v ArchiveWindow je zámerný a má limit päť sekúnd. Čaká priamo na BCL Task z HttpClient, nie na študentovu async metódu so zachyteným UI kontextom. Predvádza blokovanie UI. Aj tento pokus vytvorí publikáciu. Mimo neho nepoužívajte Result, Wait ani GetAwaiter().GetResult.

## Riešenie problémov

- CS4032: k metóde s await pridajte async, zachovajte Task alebo Task<T>.
- CS8603 pri ReadFromJsonAsync: metóda môže vrátiť null. Pridajte ?? throw new InvalidDataException(...), nevracajte null.
- Súbory sa nenačítajú: obnovte NuGet balíky a vyberte existujúci priečinok. Veľké súbory nad 32 MiB sa v reporte uvedú ako preskočené. Nedostupné podpriečinky a odkazy sa neprechádzajú.
- Testy visia alebo padnú po piatich sekundách: ochranný limit odhalil nedokončený Task. Skontrolujte odovzdanie tokenu a životnosť registrácie; nepredlžujte limit namiesto opravy.
- Core nepoužíva Result, Wait ani async void. Async void je iba hotová hranica UI udalostí, ktorá čaká na operáciu a spracúva chyby.
- Pri obmedzenom termináli môže pomôcť dotnet build FileDetective.slnx -m:1 -nr:false -p:UseSharedCompilation=false. Bežné Visual Studio tieto prepínače nepotrebuje.
- Ešte pred hodinou obnovte balíky. Pri dostupnej NuGet cache aplikácia funguje bez siete.

## Dobrovoľné rozšírenie async streamu

FileCatalog má pripravenú enumeráciu priečinka na poole, pretože Directory neposkytuje natívne async prechádzanie. Ako rozšírenie navrhnite IAsyncEnumerable<ScanProgress>, ktoré odovzdáva udalosti skenu. Porovnajte ho s hotovým streamom publikácií. Nie je to súčasť základnej 100-minútovej cesty.

Ďalšie čítanie: [CPU a I/O scenáre](https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/async-scenarios), [rušenie](https://learn.microsoft.com/en-us/dotnet/standard/threading/cancellation-in-managed-threads), [TaskCompletionSource](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.taskcompletionsource).

## Príprava systémov a kontrola testov

Na Windows je preferované Visual Studio 2026; VS Code s C# Dev Kit je alternatíva pre všetky tri systémy. Nevyžaduje sa Windows ani Visual Studio na macOS alebo Linuxe. SDK musí zodpovedať architektúre počítača. Na Macu možno namiesto klávesu F5 použiť tlačidlo Start Debugging.

Avalonia v tomto projekte na Linuxe používa X11; v relácii Wayland musí byť dostupný XWayland. Debian/Ubuntu potrebujú knižnice libx11-6, libice6, libsm6 a libfontconfig1; iné distribúcie používajú zodpovedajúce balíky. Prípravu overte pred hodinou podľa [oficiálneho zoznamu platforiem](https://docs.avaloniaui.net/docs/supported-platforms). Univerzálne príkazy dotnet run fungujú bez konfigurácie konkrétneho editora.

Po každom bloku môžete spustiť celý dotnet test FileDetective.slnx. Pre samotnú U3 napríklad použite dotnet test FileDetective.slnx --filter "FullyQualifiedName~U3"; rovnaký vzor platí pre U2, U4 a U5. Starter: 10/28, po U2: 17/28, po U3: 21/28, po U4: 25/28, po U5: 28/28. Nedoplnené neskoršie bloky zatiaľ očakávane zlyhávajú.

Rozlišujte klienta v Desktop, zdieľané služby v Core a samostatný HTTP proces Server. HTTP POST začína ArchiveClient.PublishAsync, serverová routa odovzdá RequestAborted do SaveValidAsync a ďalej do ArchiveStore.SaveAsync. Core nemení ovládacie prvky okna.

Podklady k editorom: [Visual Studio viac startup projektov](https://learn.microsoft.com/en-us/visualstudio/ide/how-to-set-multiple-startup-projects?view=visualstudio), [C# vo VS Code](https://code.visualstudio.com/docs/languages/csharp) a [ladenie C#](https://code.visualstudio.com/docs/csharp/debugging).
