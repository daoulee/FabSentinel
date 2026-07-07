using Avalonia.Data.Converters;
using Avalonia.Media;
using GreenVision.Core;
using GreenVision.Models;
using System.Globalization;

namespace GreenVision.Converters;

public sealed class AlarmLevelToColorConverter : IValueConverter
{
    public static readonly AlarmLevelToColorConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var color = value is AlarmLevel level ? level switch
        {
            AlarmLevel.Safe => AppConstants.Colors.Safe,
            AlarmLevel.Warning => AppConstants.Colors.Warning,
            AlarmLevel.Danger => AppConstants.Colors.Danger,
            _ => AppConstants.Colors.TextSecondary
        } : AppConstants.Colors.TextSecondary;

        return Color.TryParse(color, out var c) ? new SolidColorBrush(c) : Brushes.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
