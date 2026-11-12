# Laboratórne cvičenie 7 – Štartovací projekt FRI DevPulse (Avalonia UI 12 + .NET 10)

**Predmet:** Pokročilé objektové technológie (POT) – KST FRI UNIZA  
**Časová dotácia:** Orientačne 100 minút (Úlohy U1 – U4 + voliteľný Bonus)

Tento priečinok obsahuje plne kompilovateľnú štartovaciu kostru cvičenia **Lab 07**. Podrobný ilustrovaný postup nájdete v dokumente **`POT_Lab07_Navod.docx`** (resp. v prezentácii `C07-GUI-Avalonia-Studenti.pptx`).

---

## Rýchly štart (Windows, macOS, Linux)

1. Otvorte riešenie **`DevPulse.slnx`** vo **Visual Studiu 2026** (funguje ihneď cez **`F5`** aj bez nainštalovaného rozšírenia Avalonia – `DevPulse.App` je predvolený štartovací projekt), **JetBrains Rideri**, **VS Code** alebo **Google Antigravity / Cursori**.
2. Alebo overte kompiláciu, úvodný test a spustenie priamo v termináli z tohto priečinka:
   ```powershell
   dotnet build DevPulse.slnx
   dotnet test DevPulse.slnx
   dotnet run --project DevPulse.App
   ```

---

## Mapa 5 súborov, ktoré upravujete

| # | Súbor v tomto priečinku | Úloha |
| :--- | :--- | :--- |
| **1.** | `DevPulse.App/Views/MainWindow.axaml` | **U1 (Ručne):** `#ZoomSlider.Value` s `UpdateSourceTrigger=LostFocus` + `FontSize` kartičiek<br>**U2 a U3 (S AI):** TODO formulár, `$parent[Window]` tlačidlá, RSS panel a `ProgressBar` (`Grid.Column="1"`) |
| **2.** | `DevPulse.App/App.axaml` | **U1 (Ručne):** Doplnenie `Transitions` do existujúceho `Border.card` a hover štýl `Border.card:pointerover` (`BrandAccentBrush`, `CardHoverBgBrush`) |
| **3.** | `DevPulse.Core/ViewModels/MainWindowViewModel.cs` | **U2 (Ručne -> S AI):** `AddTask` (ručne + inšpekcia cez `F12` / `Cmd+Click`), `AddArticleToTasks`, `RemoveSelectedTask`, `ClearCompletedTasksAsync`<br>**U3 (S AI):** `CanLoadFeed` + `LoadFeedAsync` a `CanAnalyzeFeed` + `AnalyzeFeedAsync` (`IProgress<AnalysisProgress>` s lokálnou strážou `isActive` + `CancellationToken`) |
| **4.** | `DevPulse.App/Converters/PriorityToBrushConverter.cs` | **U2 (S AI):** Prevod `TaskPriority` na zelenú (`#059669`), jantárovú (`#D97706`) a červenú (`#DC2626`) |
| **5.** | `DevPulse.Tests/MainWindowViewModelTests.cs` | **U4 (S AI + mutačná skúška):** Automatické `xUnit` testy ViewModelu s `Fake` službami (`dotnet test`) |

**Práca s AI asistentmi:** V tomto priečinku sú pripravené súbory `AGENTS.md` a `.github/copilot-instructions.md`, ktoré vedú AI asistentov k čistému MVVM a pravidlám Avalonia UI 12. Ak používate webový chat (ChatGPT / Claude), vložte mu na úvod obsah `AGENTS.md`.

---

## Rýchla pomoc pri najčastejších chybách

- **Do poľa veľkosti písma nejde napísať `18` (skočí na `12,0`):** Pridajte do väzby `UpdateSourceTrigger=LostFocus` a po napísaní čísla `18` stlačte `Tab` alebo kliknite mimo poľa.
- **Nadpis úlohy prekrýva `CheckBox`:** V šablóne `TaskItemViewModel` v `MainWindow.axaml` iba pridajte `FontSize="{Binding #ZoomSlider.Value}"` a **neodstraňujte `Grid.Column="1"`**.
- **Chyba `AVLN2000`:** Skontrolujte názov vlastnosti vo ViewModeli, prítomnosť `x:DataType` a v šablóne článku zápis `$parent[Window].((vm:MainWindowViewModel)DataContext)`.
- **Zoznam v `DockPanel`i sa nerozťahuje alebo chýbajú spodné tlačidlá:** `<ListBox>` musí zostať v rámci `<DockPanel>` zapísaný **ako úplne posledný element** (až pod `<UniformGrid DockPanel.Dock="Bottom">`).
- **Klávesové skratky na macOS:** Prechod do vygenerovaného `*.g.cs` kódu: `Cmd + Click` (alebo `Cmd + B` v Rideri); Avalonia DevTools v bežiacom okne: `Fn + F12`.
