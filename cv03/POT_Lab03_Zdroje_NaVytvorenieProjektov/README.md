# POT Lab03 Zdroje na vlastné projekty

Balík obsahuje spoločné C# zdroje pre Windows, macOS a Linux, pripravené modely a register, textové menu a 49 xUnit testovacích prípadov. Core.csproj, App.csproj ani solution neobsahuje. Vytvorte Class Library PlaylistLab.Core a Console App PlaylistLab.App pre .NET 10 a referenciu z App na Core podľa návodu. ZIP nemusí obsahovať Directory.Build.props ani NuGet.Config.

Do Core vložte Collections, Models a Services, odstráňte Class1.cs. Do App vložte štyri .cs súbory a nahraďte Program.cs. Kopírujte obsah priečinkov bez duplicitného vnorenia. Celý pripravený PlaylistLab.Tests vložte vedľa Core a App a pridajte jeho .csproj do solution. Dokumentáciu Core zapnite v Properties > Build > Output alebo cez GenerateDocumentationFile v .csproj. PACKAGE_README.md je podklad pre dobrovoľné balenie.

Register cez Dictionary je hotový. Doplňte U2 operácie, U3 iterátory, U4 udalosť a U5 lenivý filter. Použiť môžete GitHub Copilot, Antigravity, Claude Code alebo ChatGPT/Codex. Potom vytvorte vlastnú funkciu duration alebo peek a aspoň tri vlastné testy v novom OwnFeatureTests.cs. Aspoň jeden test skúste proti úmyselne vloženej chybe a následne chybu odstráňte.

```text
dotnet build PlaylistLab.slnx
dotnet test PlaylistLab.slnx
dotnet run --project PlaylistLab.App -- --basic
dotnet run --project PlaylistLab.App -- --interactive
```

Na začiatku prechádza 23 z 49 pripravených prípadov. Po U5 prechádza všetkých 49; po vlastnej úlohe aj Vaše nové testy. NuGet tabuľka je dobrovoľný U6. Pri technickom probléme použite osobitný záložný starter ZIP.
