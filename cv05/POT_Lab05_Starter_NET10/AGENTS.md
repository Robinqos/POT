# Audit súborov pokyny pre AI

Toto je študentský postup. Výslovné pokyny vyučujúceho na úpravu zadania majú prednosť. Najprv prečítajte README.md a aktuálnu kartu v NAVOD.md. AI je povolená; urobte malú zmenu, vysvetlite predpoveď, diff a jeden hraničný prípad. Vzor je osobitne v NAPOVEDA.md.

Cieľom je skutočný nástroj na hľadanie kópií podľa obsahu a vytvorenie HTML reportu. Projekty FileDetective.Core, Desktop, Server a Tests cielia na net10.0, C# 14 a používajú FileDetective.slnx. UI, kryptografia a enumerácia sú pripravené. Päť úloh majú samostatné dialógy so spoločným vybraným priečinkom a posledným dokončeným výsledkom.

Upravujú sa Core/Fingerprint.cs, DuplicateScanner.cs, BudgetScanner.cs a ExportDecision.cs; U5 pridáva dva súbory uvedené nižšie. FileReader.cs je hotový. Dodané DetectiveTests.cs, ArchiveTests.cs a TestTools.cs nemeňte. Vlastný test patrí do OwnTests.cs. Zachovajte označené signatúry, validácie a modely.

- U1 je pripravená prehliadka. Základné async I/O z Lab04 sa znova neimplementuje.
- U2a: ThrowIfCancellationRequested v každom bloku pripravenej CPU slučky. Kontrola až po výpočte nestačí.
- U2b: await reader.ReadAsync, potom await Task.Run(() => Fingerprint.Compute(bytes, token), token), potom FingerprintEntry(path, bytes.LongLength, hash). Číta sa celý obsah, nie iba prefix. Token plánovania sám nezastaví rozbehnutý výpočet.
- U3: linked zdroj žije cez await scanner.ScanAsync. Pred spustením overte linked token. Používateľ => OCE s userToken; deadline => TimeoutException; pri oboch aktívnych príčinách má používateľ prednosť. Ostatné chyby propagujte. Dokončený výsledok sa nahrádza až po úspechu, nie po zrušení.
- U4: Task<bool> predstavuje rozhodnutie uložiť/neukladať. Registrácia tokenu cez using žije až do skončenia await na pripravený TCS. Callback TrySetCanceled(token). TryDecide vráti TrySetResult(save). Prvé dokončenie vyhrá, ďalšie vráti false. Každý nový pokus má novú bránu.

V Core nepoužívajte async void, Result, Wait ani zahodené Tasky. Async void je hotová hranica udalosti s obsluhou chýb. ConfigureAwait(false) nevytvára pracovné vlákno. Asynchrónne čítanie neobaľujte do Task.Run; CPU hashovanie je samostatná práca. Hotové prechádzanie Directory beží na poole, pretože Directory nemá natívnu async enumeráciu.

DemoData má 11 súborov, tri skupiny a štyri nadbytočné kópie. Dva plan.txt majú rovnaký názov aj veľkosť, ale odlišný obsah. Jedna kópia obrázka má iný názov. Ukážkové dáta sa pri spustení berú z koreňa riešenia, aby ich študent našiel. Vlastné pokusy robte v kópii priečinka vybranej cez Vlastný priečinok.

Starter sa zostaví; 10/28 úspešných testov, 18 očakávaných zlyhaní. Referenčné riešenie má 28/28. Testy používajú riadené brány a kontrolné body, nie tesné časovanie. Živá odozva a celý export sa overujú aj v UI. Po prvom restore netreba internet; U5 používa lokálny server.

Príkazy v priečinku so solution:
dotnet build FileDetective.slnx
dotnet run --project FileDetective.Desktop
dotnet test FileDetective.slnx

Nástroj vstupy nemení. Report vzniká ako nový HTML súbor v Reports pri spustenej aplikácii. Jeden súbor má limit 32 MiB; nedostupné a príliš veľké súbory sa uvedú medzi preskočenými. Veľký prípad 128 MiB je voliteľný a generuje sa až na kliknutie.

## U5 Tímový archív

Doplňujú sa aj Core/ArchiveClient.cs a Server/ArchiveEndpoints.cs. Server je hotová Minimal API infraštruktúra na 127.0.0.1:5085. Endpoint U5b dočká SaveAsync s RequestAborted tokenom a vráti 201 Created. Klient U5a dočká POST JSON, overí HTTP stav a dočká JSON odpoveď s rovnakým tokenom; prázdnu odpoveď (null) nevracia, ale vyhodí výnimku. Testy overujú iba klienta; endpoint sa overuje skutočnou požiadavkou v okne 5, ktoré vypíše skutočný stav a Location. GET a validácie sú hotové. Na serveri nepoužívajte Task.Run okolo asynchrónneho I/O.

MainWindow má aj piaty dialóg Tímový archív. Pomalý režim 3 s je riadená simulácia latencie. Cancel po ARCHIVE_STARTED musí viesť k ARCHIVE_CANCELLED a bez novej publikácie, ak sa ešte nezačal commit. Po dokončenom zápise zrušenie nie je rollback. Dve nezávislé požiadavky v CheckAsync tvoria pokus s WhenAll; ListAsync je pripravený async stream.

Výnimkou zo zákazu blokovania je jediný pripravený pokus v ArchiveWindow s horným limitom päť sekúnd. Čaká priamo na BCL Task a nesmie sa kopírovať do bežného kódu. WhenAll a blokovanie sú voliteľným pripomenutím z Lab04.

Terminál 1: dotnet run --project FileDetective.Server
Terminál 2: dotnet run --project FileDetective.Desktop
