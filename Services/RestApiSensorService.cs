using System.Net.Http.Json;
using System.Text.Json.Serialization;
using GreenVision.Core;
using GreenVision.Models;

namespace GreenVision.Services;

/// <summary>
/// Polls the GreenVision FastAPI server for sensor readings.
/// Compatible with GET /api/sensors/latest response format.
/// </summary>
public sealed class RestApiSensorService : ISensorDataService
{
    private readonly HttpClient _http;
    private string _baseUrl;
    private readonly int _intervalMs;

    private List<SensorReading> _current = new();
    private CancellationTokenSource? _cts;
    private bool _running;

    public IReadOnlyList<SensorReading> CurrentReadings => _current;
    public event EventHandler<IReadOnlyList<SensorReading>>? ReadingsUpdated;

    private static readonly SensorReading[] _template = new[]
    {
        new SensorReading { Name = "PM2.5",       Unit = "μg/m³", WarningThreshold = AppConstants.Thresholds.Pm25Warning,     DangerThreshold = AppConstants.Thresholds.Pm25Danger,     MinValue = 0, MaxValue = 200 },
        new SensorReading { Name = "PM10",         Unit = "μg/m³", WarningThreshold = AppConstants.Thresholds.Pm10Warning,     DangerThreshold = AppConstants.Thresholds.Pm10Danger,     MinValue = 0, MaxValue = 300 },
        new SensorReading { Name = "NH3",          Unit = "ppm",   WarningThreshold = AppConstants.Thresholds.Nh3Warning,      DangerThreshold = AppConstants.Thresholds.Nh3Danger,      MinValue = 0, MaxValue = 100 },
        new SensorReading { Name = "CO",           Unit = "ppm",   WarningThreshold = AppConstants.Thresholds.CoWarning,       DangerThreshold = AppConstants.Thresholds.CoDanger,       MinValue = 0, MaxValue = 50  },
        new SensorReading { Name = "Temperature",  Unit = "°C",    WarningThreshold = AppConstants.Thresholds.TempWarning,     DangerThreshold = AppConstants.Thresholds.TempDanger,     MinValue = 0, MaxValue = 60  },
        new SensorReading { Name = "Humidity",     Unit = "%",     WarningThreshold = AppConstants.Thresholds.HumidityWarning, DangerThreshold = AppConstants.Thresholds.HumidityDanger, MinValue = 0, MaxValue = 100 },
    };

    public RestApiSensorService(string baseUrl = "http://localhost:8000", int intervalMs = 1000)
    {
        _baseUrl   = baseUrl.TrimEnd('/');
        _intervalMs = intervalMs;
        _http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
        _current = _template.ToList();
    }

    public void UpdateBaseUrl(string url) => _baseUrl = url.TrimEnd('/');

    public async Task StartAsync(CancellationToken ct = default)
    {
        if (_running) return;
        _running = true;
        _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        _ = PollLoopAsync(_cts.Token);
        await Task.CompletedTask;
    }

    public async Task StopAsync()
    {
        _cts?.Cancel();
        _running = false;
        await Task.CompletedTask;
    }

    private async Task PollLoopAsync(CancellationToken ct)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(_intervalMs));
        while (!ct.IsCancellationRequested && await timer.WaitForNextTickAsync(ct).ConfigureAwait(false))
        {
            try
            {
                var response = await _http
                    .GetFromJsonAsync<List<ApiLatestResponse>>($"{_baseUrl}/api/sensors/latest", ct)
                    .ConfigureAwait(false);

                if (response is { Count: > 0 })
                {
                    var readings = MapToReadings(response[0]);
                    _current = readings;
                    ReadingsUpdated?.Invoke(this, readings);
                }
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[RestApiSensorService] Poll error: {ex.Message}");
            }
        }
    }

    private static List<SensorReading> MapToReadings(ApiLatestResponse response)
    {
        var map = response.Readings.ToDictionary(r => r.Name, r => r.Value);
        var ts  = DateTime.TryParse(response.Timestamp, out var dt) ? dt : DateTime.Now;

        return _template.Select(t => new SensorReading
        {
            Name             = t.Name,
            Unit             = t.Unit,
            WarningThreshold = t.WarningThreshold,
            DangerThreshold  = t.DangerThreshold,
            MinValue         = t.MinValue,
            MaxValue         = t.MaxValue,
            Value            = map.TryGetValue(t.Name, out var v) ? Math.Round(v, 2) : 0,
            Timestamp        = ts,
        }).ToList();
    }

    // ── JSON DTOs (matches FastAPI response) ──────────────────────────────────
    private sealed class ApiLatestResponse
    {
        [JsonPropertyName("device_id")]  public string DeviceId  { get; set; } = "";
        [JsonPropertyName("timestamp")]  public string Timestamp { get; set; } = "";
        [JsonPropertyName("readings")]   public List<ApiSensorReading> Readings { get; set; } = new();
    }

    private sealed class ApiSensorReading
    {
        [JsonPropertyName("name")]  public string Name  { get; set; } = "";
        [JsonPropertyName("value")] public double Value { get; set; }
        [JsonPropertyName("unit")]  public string Unit  { get; set; } = "";
    }
}
