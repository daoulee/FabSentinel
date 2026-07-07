using System.Text;
using System.Text.Json;
using GreenVision.Models;

namespace GreenVision.Services;

public sealed class SensorDataSyncService : IDisposable
{
    private readonly ISensorDataService _sensorService;
    private readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(3) };
    private const string BaseUrl = "http://localhost:8000";

    public SensorDataSyncService(ISensorDataService sensorService)
    {
        _sensorService = sensorService;
        _sensorService.ReadingsUpdated += OnReadingsUpdated;
    }

    private async void OnReadingsUpdated(object? sender, IReadOnlyList<SensorReading> readings)
    {
        try
        {
            var map = readings.ToDictionary(r => r.Name, r => Math.Round(r.Value, 2));

            var payload = new
            {
                device_id = "simulator-01",
                pm25        = Get(map, "PM2.5"),
                pm10        = Get(map, "PM10"),
                nh3         = Get(map, "NH3"),
                co          = Get(map, "CO"),
                temperature = Get(map, "Temperature"),
                humidity    = Get(map, "Humidity")
            };

            var json    = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            await _http.PostAsync($"{BaseUrl}/api/sensors", content);
        }
        catch { } // FastAPI 서버 꺼져있으면 무시
    }

    private static double Get(Dictionary<string, double> map, string key) =>
        map.TryGetValue(key, out var v) ? v : 0;

    public void Dispose()
    {
        _sensorService.ReadingsUpdated -= OnReadingsUpdated;
        _http.Dispose();
    }
}
