using GreenVision.Models;

namespace GreenVision.Services;

public interface IHardwareMonitorService
{
    IReadOnlyList<HardwareDevice> Devices { get; }
    event EventHandler<IReadOnlyList<HardwareDevice>>? DevicesUpdated;
    Task StartAsync(CancellationToken ct = default);
    Task StopAsync();
}
