using CommunityToolkit.Mvvm.ComponentModel;

namespace DevPulse.Core.ViewModels;

/// <summary>
/// Spoločný predok pre všetky ViewModely (využíva ho aj ViewLocator v Avalonii).
/// </summary>
public abstract class ViewModelBase : ObservableObject;
