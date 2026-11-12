# Architektonické pravidlá projektu (AGENTS.md)

Tento súbor slúži ako kontext pre AI asistentov a agentov (**GitHub Copilot, Google Antigravity, Cursor, Claude, ChatGPT**) pri práci na cvičení z predmetu **Pokročilé objektové technológie (FRI UNIZA)**.

## 1. Technologický stack
- **Platforma a jazyk:** `.NET 10` (`TargetFramework`: `net10.0`), `C# 14`, `<Nullable>enable</Nullable>`, `<ImplicitUsings>enable</ImplicitUsings>`.
- **GUI framework:** **Avalonia UI 12** (`Avalonia` 12.x, `FluentTheme`). Súbory pohľadov majú príponu `.axaml` a koreňový XML menný priestor `xmlns="https://github.com/avaloniaui"`.
- **MVVM knižnica:** **`CommunityToolkit.Mvvm` 8.4+** so C# Source Generátormi (`[ObservableProperty]`, `[RelayCommand]`, `[NotifyCanExecuteChangedFor]`, `[NotifyPropertyChangedFor]`). ViewModel triedy musia byť označené ako `partial` a dediť od `ObservableObject` (resp. `ViewModelBase`).
- **Dependency Injection:** `Microsoft.Extensions.Hosting` (`Host.CreateApplicationBuilder()`) a `Microsoft.Extensions.DependencyInjection` v Composition Roote (`App.axaml.cs`).
- **Unit testy:** `xUnit` v samostatnom projekte `*.Tests` (`net10.0`).

## 2. Záväzné pravidlá MVVM (View → ViewModel → Model/Service)
1. **Žiadna biznis logika v code-behind (`*.axaml.cs`):** Code-behind môže obsahovať iba čisto vizuálne záležitosti pohľadu (napr. prepnutie `RequestedThemeVariant` alebo čisto grafickú animáciu prvku). Všetok stav, validácia a aplikačné akcie patria do ViewModelu cez `ICommand` / `[RelayCommand]`.
2. **ViewModel nesmie poznať View ani UI typy:** Vo ViewModeloch nikdy nepoužívaj `Avalonia.*` ani `System.Windows.*` (žiadne `Window`, `MessageBox`, `SolidColorBrush`). Pre dialógy použi injektované rozhranie `IDialogService`, pre prevod doménového stavu na farbu použi `IValueConverter` vo vrstve `View`.
3. **Asynchrónnosť bez blokovania UI vlákna:**
   - Nikdy nepoužívaj `.Result`, `.Wait()` ani `Thread.Sleep()` na UI vlákne.
   - Nepoužívaj `async void` mimo obsluhy UI udalostí; vo ViewModeli vždy používaj `[RelayCommand] private async Task NazovAsync(CancellationToken token)`.
   - Pri dlhších operáciách podporuj zrušenie cez `[RelayCommand(IncludeCancelCommand = true)]`, blokuj súbežné spustenie cez `CanExecute` (`!IsBusy` + `[NotifyCanExecuteChangedFor]`) a hlás priebeh cez `IProgress<AnalysisProgress>` odovzdané do služby (`IFeedService.AnalyzeArticlesAsync`). V callbacku `Progress<T>` použi lokálnu synchronizovanú stráž (`lock (gate)` + `isActive` + `token.IsCancellationRequested`), aby asynchrónne doručený callback na `ThreadPool`e (v `xUnit` testoch, kde je `SynchronizationContext.Current == null`) nikdy neprepísal záverečný `StatusMessage`.

## 3. Špecifiká Avalonia UI 12 (Pozor na časté zámeny so starým WPF!)
- **Compiled Bindings sú zapnuté predvolene (`<AvaloniaUseCompiledBindingsByDefault>true</AvaloniaUseCompiledBindingsByDefault>`):**
  - Každý `Window`, `UserControl` aj `DataTemplate` **musí** mať atribút `x:DataType="..."`!
  - Pre volanie príkazu nadradeného okna z vnútra `DataTemplate` použi typovaný výraz:
    `Command="{Binding $parent[Window].((vm:MainWindowViewModel)DataContext).MojCommand}"`
- **Viditeľnosť prvkov:** Avalonia **nemá** `Visibility="Collapsed"`. Vždy používaj priamo `bool` vlastnosť `IsVisible="{Binding ...}"` (netreba `BooleanToVisibilityConverter`).
- **TextBox binding a zástupný text:** V Avalonii má `TextBox.Text` predvolený `UpdateSourceTrigger=PropertyChanged` (vhodné pre textové vstupy ako `NewTaskTitle`), no pri obojsmernom previazaní na číselný rozsah (`#ZoomSlider.Value` s `Minimum="12"`) explicitne uveď `UpdateSourceTrigger=LostFocus`, aby sa pri písaní dvojciferného čísla neorezala prvá číslica. V Avalonia 12 používaj `PlaceholderText="..."` (stará vlastnosť `Watermark` z Avalonie 11 je zastaraná – `AVLN5001`).
- **Rozloženie v `DockPanel` a `Grid`:** V `<DockPanel>` musí byť vyplňujúci prvok (`<ListBox>`) zapísaný vždy ako **posledný** (až po prvkoch s `DockPanel.Dock="Top"` a `DockPanel.Dock="Bottom"`). Pri vkladaní prvku do viacstĺpcového `<Grid>` nezabudni nastaviť `Grid.Column="..."`.
- **Element-to-Element binding:** Uprednostňuj prehľadný zápis `{Binding #NazovPrvku.Vlastnost}` (napr. `{Binding #ZoomSlider.Value, Mode=TwoWay, UpdateSourceTrigger=LostFocus}`).
- **Štýly, témy a ikony:** Používaj CSS-like selektory (`Style Selector="Border.card:pointerover"`), zdroje z `ThemeDictionaries` (`BrandAccentBrush`, `CardHoverBgBrush`) a deklaratívne `<Transitions>` namiesto WPF `Storyboard` triggerov (pri rozširovaní existujúceho štýlu zachovaj pôvodných 6 `Setter`ov pre `CornerRadius`, `Padding`, `Margin`, `BorderThickness`, `BorderBrush` a `Background`). V používateľskom rozhraní ani v stavových správach nepoužívaj emoji znaky; pre ikony používaj vektorové `PathIcon` / `StreamGeometry`.
- **DevTools v Avalonia 12:** Balík sa volá `AvaloniaUI.DiagnosticsSupport` (nie starý `Avalonia.Diagnostics`) a zapína sa v `Program.cs` cez `.WithDeveloperTools()`.
