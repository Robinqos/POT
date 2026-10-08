# POT Lab03 Záložné štartovacie riešenie

Otvorte PlaylistLab.slnx a ako spúšťaný projekt vyberte PlaylistLab.App. Hlavný návod používa vlastné projekty; toto riešenie slúži pri technickom zdržaní.

```text
dotnet build PlaylistLab.slnx
dotnet test PlaylistLab.slnx
dotnet run --project PlaylistLab.App -- --basic
```

Build uspeje. Na začiatku prechádza 23 z 49 testov a --basic hlási 4 nedokončené bloky a 0 neočakávaných chýb. TODO sú U2a/U2b, U3a/U3b, U4 a U5 filter. Register je hotový. Po spoločných blokoch doplňte vlastnú funkciu a aspoň tri testy podľa návodu. GitHub Copilot aj iné AI nástroje sú povolené. NuGet tabuľka je dobrovoľný U6, --basic ju vynecháva.

START_OD_NULY.ps1 je záložný pomocný skript iba pre Windows, hlavný postup ho nevyžaduje. Projekty majú vlastné nastavenia; spoločné Directory.Build.props a NuGet.Config nepotrebujete.
