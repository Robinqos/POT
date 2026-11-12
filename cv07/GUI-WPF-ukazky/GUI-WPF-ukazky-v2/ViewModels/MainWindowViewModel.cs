using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WpfUkazky.ViewModels;

// slajd 21 – ViewModel s ručne implementovaným INotifyPropertyChanged
class MainWindowViewModel : INotifyPropertyChanged
{
  private string city = "Hartford";
  public string City
  {
    get => city;
    set { city = value; OnPropertyChanged(); OnPropertyChanged(nameof(Summary)); }
  }

  private string country = "USA";
  public string Country
  {
    get => country;
    set { country = value; OnPropertyChanged(); OnPropertyChanged(nameof(Summary)); }
  }

  // odvodená property – mení sa spolu s City a Country
  public string Summary => $"$5.99 to {City}, {Country}";

  public event PropertyChangedEventHandler? PropertyChanged;

  void OnPropertyChanged([CallerMemberName] string? n = null)
    => PropertyChanged?.Invoke(this, new(n));
}
