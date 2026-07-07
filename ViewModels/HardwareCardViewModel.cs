using CommunityToolkit.Mvvm.ComponentModel;
using GreenVision.Core;
using GreenVision.Models;

namespace GreenVision.ViewModels;

public sealed partial class HardwareCardViewModel : ObservableObject
{
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _deviceType = string.Empty;
    [ObservableProperty] private string _icon = "●";
    [ObservableProperty] private DeviceStatus _status = DeviceStatus.Offline;
    [ObservableProperty] private string _statusText = "OFFLINE";
    [ObservableProperty] private string _statusColor = AppConstants.Colors.Danger;
    [ObservableProperty] private double _connectionQuality;
    [ObservableProperty] private string _temperature = "—";
    [ObservableProperty] private string _firmwareVersion = string.Empty;
    [ObservableProperty] private string _ipAddress = string.Empty;
    [ObservableProperty] private string _lastSeen = "Never";

    public HardwareCardViewModel(HardwareDevice device) => Update(device);

    public void Update(HardwareDevice device)
    {
        Name = device.Name;
        DeviceType = device.DeviceType;
        Icon = device.Icon;
        Status = device.Status;
        ConnectionQuality = device.ConnectionQuality;
        FirmwareVersion = device.FirmwareVersion;
        IpAddress = device.IpAddress;
        Temperature = device.Temperature.HasValue ? $"{device.Temperature:F0}°C" : "—";

        var since = device.TimeSinceLastSeen;
        LastSeen = since.TotalSeconds < 5 ? "Just now"
            : since.TotalMinutes < 1 ? $"{(int)since.TotalSeconds}s ago"
            : $"{(int)since.TotalMinutes}m ago";

        (StatusColor, StatusText) = device.Status switch
        {
            DeviceStatus.Online => (AppConstants.Colors.Safe, "ONLINE"),
            DeviceStatus.Offline => (AppConstants.Colors.Danger, "OFFLINE"),
            DeviceStatus.Connecting => (AppConstants.Colors.Warning, "CONNECTING"),
            DeviceStatus.Error => (AppConstants.Colors.Danger, "ERROR"),
            _ => (AppConstants.Colors.TextSecondary, "UNKNOWN")
        };
    }
}
