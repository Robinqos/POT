using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using DevPulse.Core.ViewModels;

namespace DevPulse.App;

/// <summary>
/// Konvencia ViewModel → View (prednáška MVVM, slajdy 13 a 15).
/// Pre triedu DevPulse.Core.ViewModels.FooViewModel automaticky vyhľadá DevPulse.App.Views.FooView.
/// </summary>
[RequiresUnreferencedCode("ViewLocator využíva reflexiu na mapovanie ViewModel -> View.")]
public sealed class ViewLocator : IDataTemplate
{
    public Control? Build(object? param)
    {
        if (param is null)
            return null;

        var viewName = param.GetType().FullName!
            .Replace(".Core.ViewModels.", ".App.Views.", StringComparison.Ordinal)
            .Replace("ViewModel", "View", StringComparison.Ordinal);

        var type = Type.GetType(viewName);
        if (type is not null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }

        return new TextBlock { Text = $"View sa nenašiel: {viewName}" };
    }

    public bool Match(object? data) => data is ViewModelBase;
}
