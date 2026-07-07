using GreenVision.Helpers;
using GreenVision.Models;

namespace GreenVision.Services;

public sealed class HardwareSimulatorService : IHardwareMonitorService, IDisposable
{
    private readonly List<HardwareDevice> _devices;
    private readonly ILogService _logService;
    private CancellationTokenSource? _cts;
    private Task? _loopTask;

    public IReadOnlyList<HardwareDevice> Devices => _devices;
    public event EventHandler<IReadOnlyList<HardwareDevice>>? DevicesUpdated;

    public HardwareSimulatorService(ILogService logService)
    {
        _logService = logService;
        _devices = new List<HardwareDevice>
        {
            new() { Name = "ESP32-WROOM",    DeviceType = "Microcontroller",       Icon = "Chip",          Status = DeviceStatus.Online, ConnectionQuality = 95,  Temperature = 42, FirmwareVersion = "v2.1.3",   IpAddress = "192.168.1.101" },
            new() { Name = "Raspberry Pi 4B",DeviceType = "Single Board Computer", Icon = "DeveloperBoard",Status = DeviceStatus.Online, ConnectionQuality = 98,  Temperature = 55, FirmwareVersion = "Raspbian 12", IpAddress = "192.168.1.100" },
            new() { Name = "USB Camera",     DeviceType = "Vision Sensor",         Icon = "Webcam",        Status = DeviceStatus.Online, ConnectionQuality = 100, FirmwareVersion = "UVC 1.5" },
            new() { Name = "Stepper Motor",  DeviceType = "Actuator",              Icon = "CogOutline",    Status = DeviceStatus.Online, ConnectionQuality = 88,  Temperature = 38, FirmwareVersion = "DRV8825" },
            new() { Name = "Tower Lamp",     DeviceType = "Indicator",             Icon = "Lightbulb",     Status = DeviceStatus.Online, ConnectionQuality = 100, FirmwareVersion = "GPIO" },
            new() { Name = "Gas Sensor Array",DeviceType = "Environmental Sensor", Icon = "Molecule",      Status = DeviceStatus.Online, ConnectionQuality = 92,  Temperature = 35, FirmwareVersion = "MQ-v3" },
        };
    }

    public Task StartAsync(CancellationToken ct = default)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        _loopTask = RunLoopAsync(_cts.Token);
        return Task.CompletedTask;
    }

    public async Task StopAsync()
    {
        if (_cts is not null)
        {
            await _cts.CancelAsync();
            if (_loopTask is not null) await _loopTask.ConfigureAwait(false);
        }
    }

    private async Task RunLoopAsync(CancellationToken ct)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(2));
        try
        {
            while (await timer.WaitForNextTickAsync(ct))
            {
                UpdateDevices();
                DevicesUpdated?.Invoke(this, _devices);
            }
        }
        catch (OperationCanceledException) { }
    }

    private void UpdateDevices()
    {
        foreach (var device in _devices)
        {
            device.LastSeen = DateTime.Now;

            // Simulate minor fluctuations
            device.ConnectionQuality = Math.Clamp(
                SimulationHelper.WalkValue(device.ConnectionQuality, 95, 1.5, 60, 100),
                60, 100);

            if (device.Temperature.HasValue)
                device.Temperature = Math.Clamp(
                    SimulationHelper.WalkValue(device.Temperature.Value, 45, 1.0, 30, 80),
                    30, 80);

            // Rare disconnect simulation
            if (SimulationHelper.Chance(0.005))
            {
                device.Status = DeviceStatus.Connecting;
                _logService.Warn(device.Name, $"{device.Name} connection lost — reconnecting");
            }
            else if (device.Status == DeviceStatus.Connecting && SimulationHelper.Chance(0.8))
            {
                device.Status = DeviceStatus.Online;
                _logService.Info(device.Name, $"{device.Name} reconnected successfully");
            }
        }
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }
}
