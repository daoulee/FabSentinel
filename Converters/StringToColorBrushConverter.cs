using Avalonia.Data.Converters;
using Avalonia.Media;
using System.Globalization;

namespace GreenVision.Converters;

public sealed class StringToColorBrushConverter : IValueConverter
{
    public static readonly StringToColorBrushConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string hex && Color.TryParse(hex, out var color))
            return new SolidColorBrush(color);
        return Brushes.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
