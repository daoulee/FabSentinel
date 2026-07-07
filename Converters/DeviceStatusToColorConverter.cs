using Avalonia.Data.Converters;
using Avalonia.Media;
using GreenVision.Core;
using GreenVision.Models;
using System.Globalization;

namespace GreenVision.Converters;

public sealed class DeviceStatusToColorConverter : IValueConverter
{
    public static readonly DeviceStatusToColorConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var hex = value is DeviceStatus s ? s switch
        {
            DeviceStatus.Online => AppConstants.Colors.Safe,
            DeviceStatus.Offline => AppConstants.Colors.Danger,
            DeviceStatus.Connecting => AppConstants.Colors.Warning,
            DeviceStatus.Error => AppConstants.Colors.Danger,
            _ => AppConstants.Colors.TextSecondary
        } : AppConstants.Colors.TextSecondary;

        return Color.TryParse(hex, out var c) ? new SolidColorBrush(c) : Brushes.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
