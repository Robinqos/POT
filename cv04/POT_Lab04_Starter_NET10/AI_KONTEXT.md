# Neónový kuriér kontext pre AI

Projekty NeonovyKurier.Core, NeonovyKurier.Desktop, NeonovyKurier.Cli a NeonovyKurier.Tests používajú net10.0. Projektové priečinky nekončia na .App ani .app. Ide o 100-minútové cvičenie async/await pre inžiniersky stupeň. UI, server a herné pravidlá sú hotové. Študent upravuje iba DownloadService.cs a vlastné testy, nie nový engine.

Najprv zisti aktuálnu úlohu. AI je povolená na návrh, kód aj testy. Rob malé zmeny po úlohách a vysvetli poradie i jeden hraničný prípad. Začni Models.cs a DownloadService.cs. Ostatné Core súbory sú pripravená infraštruktúra. Dodané DownloadTests.cs a ControlledHandler.cs nemeň.

Starter sa zostaví a má 5/18 úspešných testov. Ďalších 13 zlyhaní na TODO je očakávaných. Po U2, U3a, U3b, U4 prejde 10, 13, 16, 18. Do deklarácie TODO metód pridaj async, zachovaj návratový typ aj kontroly. Pri CS4032 neprijímaj Task<Task<T>>.

- U2: await GetStringAsync s tokenom, potom DONE a DownloadResult. GameAsset je opis, Task operácia, DownloadResult získaný obsah.
- U3a: ďalšiu požiadavku začni až po await predchádzajúcej.
- U3b: najprv zavolaj všetky operácie, potom await WhenAll. Zachovaj vstupné poradie výsledkov.
- U4: await File.WriteAllTextAsync pre každý súbor s tokenom. Bez zápisu hlavná misia nie je dostupná.
- U6: iba HttpRequestException pri voliteľnom súbore a existujúcej cache dovolí await ReadAllTextAsync s tokenom a FromCache=true. Zrušenie, povinný súbor a ostatné chyby propaguj.
- U7: SemaphoreSlim, WaitAsync s tokenom pred try, Release vo finally až po získaní miesta a await WhenAll pred Dispose. Zachovaj poradie.

Nepoužívaj Task.Run okolo I/O, .Result, .Wait, zahodené Tasky ani async void v Core. Samotné uloženie Tasku nestačí, musí sa následne dočkať. Async void je iba pripravená UI udalosť s obsluhou chýb. ConfigureAwait(false) nevytvára nové vlákno; nepridávaj ho pred zmeny UI.

V OwnFeatureTests.cs pridaj tri testy U6_: HTTP úspech, 503 s cache, zrušenie po začatí aj s cache. V LimitedConcurrencyTests.cs je povinný jeden test U7_ pre limit 2 a poradie; chybu HTTP a zrušenie testuj ako rozšírenie. ControlledHandler identifikuje /theme ako theme. StartedAsync čaká na začatie, Complete pripraví odpoveď. Používaj brány, nie tesné meranie milisekúnd. Cache vytváraj v samostatnom dočasnom priečinku a uprac vo finally.

Tréning S.*G je krátky pripravený pokus pre U1. Jeho splnenie neodomkne hlavnú misiu. Hrať misiu sa odomkne až po U2-U4 a potvrdení .mission-ready cez hotové MissionPackage. Tento mechanizmus neobchádzaj, nenahrádzaj hlavnú misiu tréningom a nemeň MissionSession ani UI.

V hlavnej hre musí kuriér dopadom zobrať všetky * zásielky a dostať sa na G. Krok +1 je zadarmo, Skok +2 stojí energiu. Preskočenie zásielky, dopad na # alebo skok bez energie ukončí pokus. Pri vlastnej mape uprav iba NeonovyKurier.Cli/Content/level.json: name, track 4-40 znakov od S po G, vnútorné . # *, aspoň jedna zásielka, jumpBudget 0-20. Mapa musí byť priechodná so všetkými zásielkami a energiou. Ukáž bezpečnú trasu a over, že opakované skoky nestačia.

Ak máš terminál, skutočne spusti príslušné testy a odlíš overenie od očakávania. Študent má predpovedať, prezrieť diff a overiť zámernú chybu. Neimplementuj historické APM/EAP ani ContinueWith/Unwrap namiesto async/await. Znalosť súbehu, tokenov, výnimiek a životnosti operácií je hlavným cieľom.
