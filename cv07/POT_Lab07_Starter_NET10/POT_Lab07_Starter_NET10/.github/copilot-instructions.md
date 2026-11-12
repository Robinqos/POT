Pri generovaní alebo úprave kódu v tomto repozitári vždy dodržiavaj architektonické pravidlá uvedené v súbore `AGENTS.md`:

1. Používaj **.NET 10**, **C# 14**, **Avalonia UI 12** (`.axaml`) a **`CommunityToolkit.Mvvm`** (`[ObservableProperty]`, `[RelayCommand]`).
2. Vždy pridávaj `x:DataType` na `Window`, `UserControl` aj každý `DataTemplate` (Compiled Bindings sú aktívne).
3. Nikdy nepoužívaj WPF-špecifické ani zastarané Avalonia 11 konštrukcie (`Visibility="Collapsed"`, `MessageBox.Show`, `System.Windows.*`, `XmlDataProvider`, `Avalonia.Diagnostics`, `TextBox.Watermark`). Namiesto `Visibility` použi `IsVisible="{Binding ...}"`, namiesto `Watermark` použi `PlaceholderText="..."` a namiesto `ElementName` preferuj `#ElementName.Property`.
4. Striktné MVVM: ViewModel nesmie referencovať `Avalonia.*` ani `Window`. Dialógy rieš cez `IDialogService` a farby/štýly cez `IValueConverter` alebo CSS-like Avalonia `Style` selektory.
5. Všetky I/O a sieťové operácie píš asynchrónne (`async Task`) s podporou `CancellationToken` a `IProgress<T>` odovzdaného do služby (`IFeedService.AnalyzeArticlesAsync`). V callbacku `Progress<T>` použi lokálnu synchronizovanú stráž (`lock (gate)` + `isActive`), aby oneskorený `ThreadPool` callback v testoch neprepísal záverečný status.
6. Pri obojsmernom previazaní `TextBox` so `Slider` (`Minimum="12"`) pridaj `UpdateSourceTrigger=LostFocus`. V `DockPanel` zapisuj hlavný `<ListBox>` ako posledného potomka (`LastChildFill`) a na `ProgressBar` v stavovom riadku uveď `Grid.Column="1"`.
7. Ku každej novej logike vo ViewModeli navrhni zodpovedajúci `xUnit` test s falošnou (Fake) implementáciou služieb.
