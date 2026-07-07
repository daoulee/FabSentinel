using Avalonia.Data.Converters;
using Avalonia.Media;
using GreenVision.Core;
using GreenVision.Models;
using System.Globalization;

namespace GreenVision.Converters;

public sealed class LogSeverityToColorConverter : IValueConverter
{
    public static readonly LogSeverityToColorConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var hex = value is LogSeverity s ? s switch
        {
            LogSeverity.Info => AppConstants.Colors.TextSecondary,
            LogSeverity.Warning => AppConstants.Colors.Warning,
            LogSeverity.Error => AppConstants.Colors.Danger,
            LogSeverity.Critical => AppConstants.Colors.Danger,
            _ => AppConstants.Colors.TextSecondary
        } : AppConstants.Colors.TextSecondary;

        return Color.TryParse(hex, out var c) ? new SolidColorBrush(c) : Brushes.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
