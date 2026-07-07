namespace GreenVision.Models;

public enum DeviceStatus { Online, Offline, Connecting, Error }

public sealed class HardwareDevice
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string DeviceType { get; init; } = string.Empty;
    public string Icon { get; init; } = "●";
    public DeviceStatus Status { get; set; } = DeviceStatus.Offline;
    public double ConnectionQuality { get; set; }
    public double? Temperature { get; set; }
    public string FirmwareVersion { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public DateTime LastSeen { get; set; } = DateTime.Now;
    public string StatusMessage { get; set; } = string.Empty;

    public string StatusText => Status switch
    {
        DeviceStatus.Online => "ONLINE",
        DeviceStatus.Offline => "OFFLINE",
        DeviceStatus.Connecting => "CONNECTING",
        DeviceStatus.Error => "ERROR",
        _ => "UNKNOWN"
    };

    public TimeSpan TimeSinceLastSeen => DateTime.Now - LastSeen;
}
