# Audit súborov starter

Windows: Visual Studio 2026; Windows/macOS/Linux: VS Code s C# Dev Kit alebo terminál.
Otvorte celý priečinok s FileDetective.slnx. Profily Audit - server a okno spustia Server aj Desktop.
Najprv overte .NET 10 SDK a vykonajte dotnet restore FileDetective.slnx.
Päť samostatných dialógov pracuje s tým istým priečinkom, výsledkom a tímovým archívom.
Úlohy sú v NAVOD.md, nápoveda osobitne v NAPOVEDA.md.

dotnet build FileDetective.slnx
Terminál 1: dotnet run --project FileDetective.Server
Terminál 2:
dotnet run --project FileDetective.Desktop
dotnet test FileDetective.slnx

DemoData obsahuje 11 súborov, 3 skupiny rovnakého obsahu a 4 nadbytočné kópie.
Server na 127.0.0.1:5085 potrebuje iba úloha U5; lokálne hľadanie funguje aj bez neho.
Starter sa zostaví, TODO sa dopĺňajú podľa pracovných kariet.
