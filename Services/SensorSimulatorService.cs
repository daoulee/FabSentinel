using GreenVision.Core;
using GreenVision.Helpers;
using GreenVision.Models;

namespace GreenVision.Services;

public sealed class SensorSimulatorService : ISensorDataService, IDisposable
{
    private readonly List<SensorReading> _readings;
    private readonly ILogService _logService;
    private CancellationTokenSource? _cts;
    private Task? _loopTask;

    public IReadOnlyList<SensorReading> CurrentReadings => _readings;
    public event EventHandler<IReadOnlyList<SensorReading>>? ReadingsUpdated;

    public SensorSimulatorService(ILogService logService)
    {
        _logService = logService;
        _readings = new List<SensorReading>
        {
            new() { Name = AppConstants.Sensors.Pm25, Unit = "μg/m³", Value = 15.0, WarningThreshold = AppConstants.Thresholds.Pm25Warning, DangerThreshold = AppConstants.Thresholds.Pm25Danger, MinValue = 0, MaxValue = 200 },
            new() { Name = AppConstants.Sensors.Pm10, Unit = "μg/m³", Value = 30.0, WarningThreshold = AppConstants.Thresholds.Pm10Warning, DangerThreshold = AppConstants.Thresholds.Pm10Danger, MinValue = 0, MaxValue = 300 },
            new() { Name = AppConstants.Sensors.Nh3, Unit = "ppm", Value = 5.0, WarningThreshold = AppConstants.Thresholds.Nh3Warning, DangerThreshold = AppConstants.Thresholds.Nh3Danger, MinValue = 0, MaxValue = 100 },
            new() { Name = AppConstants.Sensors.Co, Unit = "ppm", Value = 3.0, WarningThreshold = AppConstants.Thresholds.CoWarning, DangerThreshold = AppConstants.Thresholds.CoDanger, MinValue = 0, MaxValue = 50 },
            new() { Name = AppConstants.Sensors.Temperature, Unit = "°C", Value = 22.0, WarningThreshold = AppConstants.Thresholds.TempWarning, DangerThreshold = AppConstants.Thresholds.TempDanger, MinValue = -10, MaxValue = 60 },
            new() { Name = AppConstants.Sensors.Humidity, Unit = "%", Value = 55.0, WarningThreshold = AppConstants.Thresholds.HumidityWarning, DangerThreshold = AppConstants.Thresholds.HumidityDanger, MinValue = 0, MaxValue = 100 },
        };
    }

    public Task StartAsync(CancellationToken ct = default)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        _loopTask = RunLoopAsync(_cts.Token);
        _logService.Info("SensorService", "Sensor simulation started");
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
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(AppConstants.Simulation.UpdateIntervalMs));
        try
        {
            while (await timer.WaitForNextTickAsync(ct))
            {
                UpdateReadings();
                ReadingsUpdated?.Invoke(this, _readings);
            }
        }
        catch (OperationCanceledException) { }
    }

    private static readonly (string Name, double Base, double StdDev)[] _simParams =
    {
        (AppConstants.Sensors.Pm25, 15.0, 0.6),
        (AppConstants.Sensors.Pm10, 30.0, 1.0),
        (AppConstants.Sensors.Nh3, 5.0, 0.2),
        (AppConstants.Sensors.Co, 3.0, 0.1),
        (AppConstants.Sensors.Temperature, 22.0, 0.1),
        (AppConstants.Sensors.Humidity, 55.0, 0.4),
    };

    private void UpdateReadings()
    {
        foreach (var reading in _readings)
        {
            var param = Array.Find(_simParams, p => p.Name == reading.Name);
            reading.Value = SimulationHelper.WalkValue(
                reading.Value, param.Base, param.StdDev, reading.MinValue, reading.MaxValue);
            reading.Timestamp = DateTime.Now;
        }
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }
}
