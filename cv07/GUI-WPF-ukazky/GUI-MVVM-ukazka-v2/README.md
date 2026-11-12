# MVVM – ukážky k prednáške (P08-GUI-MVVM)

Riešenie pre **Visual Studio 2026** a **.NET 10** (`net10.0-windows`, C# 14).
Otvorte `MvvmUkazka.slnx`. Spúšťaný projekt zvoľte v Solution Exploreri (*Set as Startup Project*).

Obe aplikácie zobrazujú **ten istý formulár** (meno, priezvisko, e-mail, tlačidlo Ulož), raz s Caliburn.Micro, raz s CommunityToolkit.Mvvm –
rovnako ako slajdy 18 až 20.

| Projekt | Obsah | Slajdy |
|---|---|---|
| `MvvmCaliburn` | Caliburn.Micro 5 – Bootstrapper so `SimpleContainer`, konvencie `x:Name` ↔ property/metóda, `CanCmdSave`, `TryCloseAsync` | 15 – 19 |
| `MvvmToolkit` | CommunityToolkit.Mvvm – `[ObservableProperty]`, `[RelayCommand]`, `[NotifyCanExecuteChangedFor]`; DI cez Generic Host v `App.xaml.cs` | 20 |
| `MvvmSpolocne` | `IDialogService` (rozhranie) a `DialogService` (implementácia cez `MessageBox`) | 19 |
| `MvvmTesty` | xUnit testy oboch ViewModelov s `FakeDialogs` – bez okna a bez kliknutia | 19, 21 |

## Rozloženie súborov (slajd 16)

```
MvvmCaliburn/
  App.xaml              – bez StartupUri
  App.xaml.cs           – v konštruktore vytvorí Bootstrapper
  Bootstrapper.cs       – registrácia služieb a ViewModelov
  ViewModels/ShellViewModel.cs
  Views/ShellView.xaml  – Caliburn.Micro nájde View podľa mena: ...ViewModels.ShellViewModel → ...Views.ShellView
```

## Poznámky

- **Caliburn.Micro 5.0** je aktuálna stabilná verzia (2025). Kód zo slajdov (písaný pre 4.x) funguje bez zmien.
- **Bootstrapper** sa vytvára v konštruktore `App` (`App.xaml.cs`). Zápis `<local:Bootstrapper x:Key="Bootstrapper" />`
  priamo v `Application.Resources` nefunguje: WPF taký zdroj vytvorí až pri prvom použití – teda nikdy – a okno sa nezobrazí.
  (Funguje iba vo vnorenom `ResourceDictionary.MergedDictionaries`, čo je zbytočne zložité.)
- **Testy** spustíte cez *Test → Run All Tests* alebo príkazom `dotnet test`.
