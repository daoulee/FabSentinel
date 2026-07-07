using GreenVision.Models;

namespace GreenVision.Services;

public interface ISensorDataService
{
    IReadOnlyList<SensorReading> CurrentReadings { get; }
    event EventHandler<IReadOnlyList<SensorReading>>? ReadingsUpdated;
    Task StartAsync(CancellationToken ct = default);
    Task StopAsync();
}
