using Avalonia.Controls;
using Avalonia.Controls.Templates;
using GreenVision.Core;

namespace GreenVision;

public sealed class ViewLocator : IDataTemplate
{
    public Control? Build(object? data)
    {
        if (data is null) return null;

        var vmTypeName = data.GetType().FullName!;
        var viewTypeName = vmTypeName
            .Replace(".ViewModels.", ".Views.", StringComparison.Ordinal)
            .Replace("ViewModel", "View", StringComparison.Ordinal);

        var type = Type.GetType(viewTypeName);
        if (type is not null)
        {
            var control = (Control)Activator.CreateInstance(type)!;
            control.DataContext = data;
            return control;
        }

        return new TextBlock
        {
            Text = $"View not found: {viewTypeName}",
            Foreground = Avalonia.Media.Brushes.OrangeRed,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
        };
    }

    public bool Match(object? data) => data is ViewModelBase;
}
