using CommunityToolkit.Mvvm.ComponentModel;
using GreenVision.Core;
using GreenVision.Models;

namespace GreenVision.ViewModels;

public sealed partial class SensorCardViewModel : ObservableObject
{
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _unit = string.Empty;
    [ObservableProperty] private double _value;
    [ObservableProperty] private string _displayValue = "—";
    [ObservableProperty] private AlarmLevel _alarmLevel = AlarmLevel.Safe;
    [ObservableProperty] private string _statusColor = AppConstants.Colors.Safe;
    [ObservableProperty] private string _statusText = "SAFE";
    [ObservableProperty] private double _normalizedValue;
    [ObservableProperty] private string _icon = "●";
    [ObservableProperty] private double _warningThreshold;
    [ObservableProperty] private double _dangerThreshold;
    [ObservableProperty] private double _maxValue;

    public SensorCardViewModel(string name, string unit, double warningThreshold, double dangerThreshold, double maxValue, string icon = "●")
    {
        _name = name;
        _unit = unit;
        _warningThreshold = warningThreshold;
        _dangerThreshold = dangerThreshold;
        _maxValue = maxValue;
        _icon = icon;
    }

    public void Update(SensorReading reading)
    {
        Value = reading.Value;
        DisplayValue = $"{reading.Value:F1}";
        AlarmLevel = reading.AlarmLevel;
        NormalizedValue = reading.NormalizedValue;

        (StatusColor, StatusText) = reading.AlarmLevel switch
        {
            AlarmLevel.Safe => (AppConstants.Colors.Safe, "SAFE"),
            AlarmLevel.Warning => (AppConstants.Colors.Warning, "WARNING"),
            AlarmLevel.Danger => (AppConstants.Colors.Danger, "DANGER"),
            _ => (AppConstants.Colors.Safe, "SAFE")
        };
    }
}
