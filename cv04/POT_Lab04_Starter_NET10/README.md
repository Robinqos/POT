# Neónový kuriér

Otvorte NeonovyKurier.slnx. Potrebujete .NET 10 SDK. Hlavný klient je NeonovyKurier.Desktop, konzolová záloha NeonovyKurier.Cli. Prvý restore potrebuje internet alebo NuGet cache.

```text
dotnet restore NeonovyKurier.slnx
dotnet build NeonovyKurier.slnx
dotnet run --project NeonovyKurier.Desktop
dotnet test NeonovyKurier.slnx
```

Hlavná misia sa sprístupní po Stiahnuť misiu, úspešnom zápise a validácii .mission-ready. Predtým funguje iba krátky Tréning ovládania, jeho výhra hlavnú hru neodomkne. Všetky zásielky sú povinné. Krok +1 je zadarmo, Skok +2 spotrebuje energiu. Preskočená zásielka, stena či skok bez energie ukončí pokus. Od začiatku obnoví hru. Pravidlá a UI sú pripravené v Mission.cs, MissionPackage.cs a Desktop; nejde o ďalšie TODO.

Súbory mapy sú v NeonovyKurier.Cli/Content/level.json: name, track a jumpBudget. Po úprave aplikáciu zatvorte, spustite znovu a znova stiahnite misiu. Mapa musí byť priechodná so všetkými zásielkami a energiou. Ovládanie Sleep/Await a ďalšie pokusy sú v Pokusy a diagnostika. Najprv otvorte tréning, aby ste mohli sledovať odozvu hry počas prenosu.

Konzola: --training, --trace, --one, --sequential, --concurrent, --install, --failure, --cancel, --recover, --play, --check, --basic. V hre Enter=krok, j=skok, r=od začiatku, q=koniec. Konzola a okno majú vlastný installed; získavajte aj hrajte v tom istom klientovi. --basic overuje iba rozbeh U2-U4. Thread.Sleep je iba zámerný krátky pokus U1.

AI môže pomáhať s návrhom, kódom a testami; kontext je v AI_KONTEXT.md a .github/copilot-instructions.md. Prezrite diff a skutočne spustite kontrolu. Riešenie sa neodovzdáva. Pri Markdown návode zachovajte assets.

Starter sa zostaví a má 5/18 úspešných testov. Ostatných 13 zlyhaní na TODO je očakávaných. Dopĺňajte Core/DownloadService.cs po úlohách U2, U3a, U3b, U4, U6, U7; pridajte async do deklarácie a zachovajte kontroly. Dodané DownloadTests.cs a ControlledHandler.cs nemeňte. Po U2-U4 prejde 18/18, po troch vlastných testoch U6 a jednom U7 aspoň 22/22. Presný postup je v NAVOD.md alebo samostatnom PDF.
