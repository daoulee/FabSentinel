using System.IO.Ports;
using System.Text.Json;
using System.Text.Json.Serialization;
using GreenVision.Core;
using GreenVision.Models;

namespace GreenVision.Services;

/// <summary>
/// Reads JSON sensor data from ESP32 over a serial (USB) port.
///
/// Expected ESP32 JSON format (one line per sample):
/// {"pm25":12.3,"pm10":18.0,"nh3":4.1,"co":2.5,"temperature":22.1,"humidity":55.3}
/// </summary>
public sealed class SerialPortSensorService : ISensorDataService
{
    private SerialPort? _port;
    private string _portName;
    private readonly int _baudRate;

    private List<SensorReading> _current;
    private CancellationTokenSource? _cts;

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

    public SerialPortSensorService(string portName = "COM3", int baudRate = 115200)
    {
        _portName = portName;
        _baudRate = baudRate;
        _current  = _template.ToList();
    }

    public void UpdatePort(string portName) => _portName = portName;

    public Task StartAsync(CancellationToken ct = default)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        _ = ReadLoopAsync(_cts.Token);
        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        _cts?.Cancel();
        _port?.Close();
        _port?.Dispose();
        _port = null;
        return Task.CompletedTask;
    }

    private async Task ReadLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                _port = new SerialPort(_portName, _baudRate)
                {
                    ReadTimeout  = 3000,
                    WriteTimeout = 1000,
                    NewLine      = "\n",
                };
                _port.Open();
                Console.WriteLine($"[SerialPort] Connected: {_portName} @ {_baudRate}");

                while (!ct.IsCancellationRequested)
                {
                    string line = await Task.Run(() => _port.ReadLine(), ct).ConfigureAwait(false);
                    line = line.Trim();
                    if (string.IsNullOrEmpty(line) || line[0] != '{') continue;

                    var payload = JsonSerializer.Deserialize<Esp32Payload>(line);
                    if (payload is null) continue;

                    var readings = MapToReadings(payload);
                    _current = readings;
                    ReadingsUpdated?.Invoke(this, readings);
                }
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[SerialPort] Error ({_portName}): {ex.Message}");
                _port?.Close();
                _port?.Dispose();
                _port = null;

                // Retry after 3 seconds
                try { await Task.Delay(3000, ct); } catch { break; }
            }
        }
    }

    private static List<SensorReading> MapToReadings(Esp32Payload p)
    {
        var now = DateTime.Now;
        return new List<SensorReading>
        {
            MakeReading("PM2.5",       "μg/m³", p.Pm25,        AppConstants.Thresholds.Pm25Warning,     AppConstants.Thresholds.Pm25Danger,     0, 200,  now),
            MakeReading("PM10",        "μg/m³", p.Pm10,        AppConstants.Thresholds.Pm10Warning,     AppConstants.Thresholds.Pm10Danger,     0, 300,  now),
            MakeReading("NH3",         "ppm",   p.Nh3,         AppConstants.Thresholds.Nh3Warning,      AppConstants.Thresholds.Nh3Danger,      0, 100,  now),
            MakeReading("CO",          "ppm",   p.Co,          AppConstants.Thresholds.CoWarning,       AppConstants.Thresholds.CoDanger,       0, 50,   now),
            MakeReading("Temperature", "°C",    p.Temperature, AppConstants.Thresholds.TempWarning,     AppConstants.Thresholds.TempDanger,     0, 60,   now),
            MakeReading("Humidity",    "%",     p.Humidity,    AppConstants.Thresholds.HumidityWarning, AppConstants.Thresholds.HumidityDanger, 0, 100,  now),
        };
    }

    private static SensorReading MakeReading(
        string name, string unit, double value,
        double warn, double danger, double min, double max, DateTime ts)
        => new()
        {
            Name = name, Unit = unit,
            Value = Math.Round(value, 2),
            WarningThreshold = warn, DangerThreshold = danger,
            MinValue = min, MaxValue = max,
            Timestamp = ts,
        };

    // ── ESP32 JSON payload ────────────────────────────────────────────────────
    private sealed class Esp32Payload
    {
        [JsonPropertyName("pm25")]        public double Pm25        { get; set; }
        [JsonPropertyName("pm10")]        public double Pm10        { get; set; }
        [JsonPropertyName("nh3")]         public double Nh3         { get; set; }
        [JsonPropertyName("co")]          public double Co          { get; set; }
        [JsonPropertyName("temperature")] public double Temperature { get; set; }
        [JsonPropertyName("humidity")]    public double Humidity    { get; set; }
    }
}
