# WPF – ukážky k prednáške (P08-GUI-WPF)

Projekt pre **Visual Studio 2026** a **.NET 10** (`net10.0-windows`, C# 14).
Otvorte `WpfUkazky.slnx` a spustite (F5). Hlavné okno obsahuje tlačidlá ku všetkým ukážkam.

| Slajd | Téma | Súbor |
|---|---|---|
| 7, 8, 11, 12 | XAML – kompozícia, Content atribút a kolekcia | `Okna/W01_Kompozicia.xaml` |
| 13 | Code-behind – obsluha udalosti, `int.TryParse` | `Okna/W02_CodeBehind.xaml(.cs)` |
| 14 | Code-behind – udalosti pripojené v konštruktore | `Okna/W03_CodeBehindDynamika.xaml(.cs)` |
| 15 | Zmena GUI z iného vlákna → `InvalidOperationException` | `Okna/W04_VlaknaPriamo.xaml.cs` |
| 16 | Preposlanie do UI vlákna cez `Dispatcher.Invoke` | `Okna/W05_VlaknaDispatcher.xaml.cs` |
| 17 – 19 | `Task.Run`, `async/await`, `Progress<T>`, `CancellationToken` | `Okna/W06_VlaknaAsync.xaml.cs` |
| 21 | ViewModel s `INotifyPropertyChanged`, `UpdateSourceTrigger` | `ViewModels/MainWindowViewModel.cs`, `Okna/W07_DataBindingCity.xaml` |
| 25 | Binding na iný prvok (`ElementName`) | `Okna/W08_BindingSlider.xaml`, `Okna/W09_BindingElipsa.xaml` |
| 29 | Štýly – v prvku, pomenovaný, implicitný | `Okna/W10_Styly.xaml` |
| 30 – 32 | Aplikácia bez code-behind, dáta vo ViewModeli, `DataTemplate` | `ViewModels/LibraryViewModel.cs`, `Okna/KnihyView.xaml`, `Okna/W11_Knihy.xaml` |
| 33, 34 | Animácie – `Storyboard`, viac animácií naraz | `Okna/W12_Animacia1.xaml`, `Okna/W13_Animacia2.xaml` |
| 35 – 37 | 3D – `Viewport2DVisual3D` s interaktívnymi prvkami | `Okna/W14_3D_Tlacidla.xaml`, `Okna/W15_3D_Formular.xaml` |
| 38 | Multimédiá – `MediaElement`, relatívne cesty | `Okna/W16_Multimedia.xaml`, priečinok `Media` |

## Poznámky

- **Slajd 15:** ukážku spustite v debuggeri (F5). Visual Studio sa zastaví na `InvalidOperationException`,
  rovnako ako na snímke v prezentácii. Bez debuggera výnimka „zapadne“ v nesledovanom `Task`.
- **Slajd 37:** 3D formulár používa ten istý `KnihyView` ako ukážka Knihy – v 3D ostáva plne funkčný
  (výber v zozname, úprava textu). Uhol natočenia sa mení posuvníkom.
- **Slajd 38:** videá sú vo formáte WMV (Windows Media). `MediaElement` ich prehrá na každom Windows.
  Súbory majú v projekte nastavené *Copy to Output Directory*, preto stačia relatívne cesty `Media/...`.
- Staré stránky `Wpf_IE-*.xaml` (XBAP / spúšťanie v Internet Exploreri) boli vynechané – táto technológia už neexistuje.
