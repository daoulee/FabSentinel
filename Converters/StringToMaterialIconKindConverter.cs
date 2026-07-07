using System.Globalization;
using Avalonia.Data.Converters;
using Material.Icons;

namespace GreenVision.Converters;

public class StringToMaterialIconKindConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string s && Enum.TryParse<MaterialIconKind>(s, out var kind))
            return kind;
        return MaterialIconKind.Circle;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
