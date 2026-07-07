using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GreenVision.Core;
using GreenVision.Models;
using GreenVision.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Collections.ObjectModel;

namespace GreenVision.ViewModels;

public sealed partial class FabMonitoringViewModel : ViewModelBase
{
    private readonly ISensorDataService _sensorService;

    public SensorCardViewModel[] AllSensors { get; }

    private readonly ObservableCollection<double> _tempHistory = new();
    private readonly ObservableCollection<double> _humidHistory = new();
    private readonly ObservableCollection<double> _pm25History = new();
    private readonly ObservableCollection<double> _nh3History = new();

    public ISeries[] EnvironmentSeries { get; }
    public ISeries[] GasSeries { get; }
    public Axis[] XAxes { get; } = new Axis[] { new Axis { IsVisible = false, ShowSeparatorLines = false } };
    public Axis[] YAxes { get; }

    [ObservableProperty] private string _selectedFabTab = "Monitor";
    public bool IsMonitorTab   => SelectedFabTab == "Monitor";
    public bool IsFloorPlanTab => SelectedFabTab == "FloorPlan";

    [ObservableProperty] private string _cctvTimestamp = DateTime.Now.ToString("yyyy-MM-dd  HH:mm:ss");
    private readonly System.Timers.Timer _clockTimer;

    partial void OnSelectedFabTabChanged(string value)
    {
        OnPropertyChanged(nameof(IsMonitorTab));
        OnPropertyChanged(nameof(IsFloorPlanTab));
    }

    [RelayCommand]
    private void SelectFabTab(string tab) => SelectedFabTab = tab;

    [ObservableProperty] private string _riskLevel = "LOW";
    [ObservableProperty] private string _riskColor = AppConstants.Colors.Safe;
    [ObservableProperty] private double _riskScore;
    [ObservableProperty] private string _riskDescription = "All parameters within normal operating range.";

    public string RiskIcon => RiskLevel switch
    {
        "MEDIUM" => "AlertOutline",
        "HIGH"   => "AlertOctagonOutline",
        _        => "ShieldCheckOutline"
    };

    partial void OnRiskLevelChanged(string value) =>
        OnPropertyChanged(nameof(RiskIcon));
    [ObservableProperty] private ObservableCollection<LogEntry> _alarmHistory = new();
    [ObservableProperty] private string _selectedSensorName = "Temperature";
    [ObservableProperty] private double _calibrationOffset;

    public FabMonitoringViewModel(ISensorDataService sensorService, IAlarmService alarmService)
    {
        _sensorService = sensorService;

        AllSensors = new[]
        {
            new SensorCardViewModel("PM2.5", "μg/m³", AppConstants.Thresholds.Pm25Warning,     AppConstants.Thresholds.Pm25Danger,     200, "Grain"),
            new SensorCardViewModel("PM10",  "μg/m³", AppConstants.Thresholds.Pm10Warning,     AppConstants.Thresholds.Pm10Danger,     300, "Blur"),
            new SensorCardViewModel("NH3",   "ppm",   AppConstants.Thresholds.Nh3Warning,      AppConstants.Thresholds.Nh3Danger,      100, "Flask"),
            new SensorCardViewModel("CO",    "ppm",   AppConstants.Thresholds.CoWarning,       AppConstants.Thresholds.CoDanger,       50,  "Smoke"),
            new SensorCardViewModel("Temp",  "°C",    AppConstants.Thresholds.TempWarning,     AppConstants.Thresholds.TempDanger,     60,  "Thermometer"),
            new SensorCardViewModel("Humid", "%",     AppConstants.Thresholds.HumidityWarning, AppConstants.Thresholds.HumidityDanger, 100, "WaterPercent"),
        };

        var rng = Random.Shared;
        for (int i = 0; i < 60; i++)
        {
            _tempHistory.Add(Math.Round(22 + rng.NextDouble() * 4, 1));
            _humidHistory.Add(Math.Round(52 + rng.NextDouble() * 8, 1));
            _pm25History.Add(Math.Round(12 + rng.NextDouble() * 10, 1));
            _nh3History.Add(Math.Round(4 + rng.NextDouble() * 3, 2));
        }

        EnvironmentSeries = new ISeries[]
        {
            new LineSeries<double> { Name = "Temperature", Values = _tempHistory, Fill = null, Stroke = new SolidColorPaint(new SKColor(0x00, 0xD2, 0xB5)) { StrokeThickness = 2 }, GeometrySize = 0 },
            new LineSeries<double> { Name = "Humidity",    Values = _humidHistory, Fill = null, Stroke = new SolidColorPaint(new SKColor(0x38, 0xBD, 0xF8)) { StrokeThickness = 2 }, GeometrySize = 0 },
        };

        GasSeries = new ISeries[]
        {
            new LineSeries<double> { Name = "PM2.5", Values = _pm25History, Fill = null, Stroke = new SolidColorPaint(new SKColor(0xEF, 0x44, 0x44)) { StrokeThickness = 2 }, GeometrySize = 0 },
            new LineSeries<double> { Name = "NH3",   Values = _nh3History,  Fill = null, Stroke = new SolidColorPaint(new SKColor(0xA8, 0x5C, 0xFF)) { StrokeThickness = 2 }, GeometrySize = 0 },
        };

        YAxes = new Axis[] { new Axis { LabelsPaint = new SolidColorPaint(new SKColor(0x94, 0xA3, 0xB8)), SeparatorsPaint = new SolidColorPaint(new SKColor(0x1E, 0x29, 0x3B)) } };

        _clockTimer = new System.Timers.Timer(1000);
        _clockTimer.Elapsed += (_, _) =>
            Dispatcher.UIThread.Post(() =>
                CctvTimestamp = DateTime.Now.ToString("yyyy-MM-dd  HH:mm:ss"));
        _clockTimer.Start();

        alarmService.AlarmLevelChanged += (_, level) => UpdateRisk(level);
        _sensorService.ReadingsUpdated += OnReadingsUpdated;
    }

    private void OnReadingsUpdated(object? sender, IReadOnlyList<SensorReading> readings)
    {
        Dispatcher.UIThread.Post(() =>
        {
            var sensorMap = new Dictionary<string, string>
            {
                ["PM2.5"] = "PM2.5", ["PM10"] = "PM10", ["NH3"] = "NH3",
                ["CO"] = "CO", ["Temperature"] = "Temp", ["Humidity"] = "Humid"
            };

            foreach (var r in readings)
            {
                if (sensorMap.TryGetValue(r.Name, out var cardName))
                {
                    var card = Array.Find(AllSensors, c => c.Name == cardName);
                    card?.Update(r);
                }

                switch (r.Name)
                {
                    case "Temperature": Push(_tempHistory, r.Value, 1); break;
                    case "Humidity":    Push(_humidHistory, r.Value, 1); break;
                    case "PM2.5":       Push(_pm25History, r.Value, 1); break;
                    case "NH3":         Push(_nh3History, r.Value, 2); break;
                }
            }
        });
    }

    private void UpdateRisk(AlarmLevel level)
    {
        Dispatcher.UIThread.Post(() =>
        {
            (RiskLevel, RiskColor, RiskScore, RiskDescription) = level switch
            {
                AlarmLevel.Safe => ("LOW", AppConstants.Colors.Safe, 15, "All parameters within normal operating range."),
                AlarmLevel.Warning => ("MEDIUM", AppConstants.Colors.Warning, 55, "One or more sensors approaching threshold. Monitor closely."),
                AlarmLevel.Danger => ("HIGH", AppConstants.Colors.Danger, 88, "CRITICAL: Immediate action required. Sensor values exceed safety thresholds."),
                _ => ("LOW", AppConstants.Colors.Safe, 0, "")
            };
        });
    }

    [RelayCommand]
    private void ApplyCalibration()
    {
        // Placeholder: actual calibration would update sensor offsets
    }

    private static void Push(ObservableCollection<double> col, double val, int decimals = 1)
    {
        const double alpha = 0.25;
        double smoothed = col.Count > 0 ? alpha * val + (1 - alpha) * col[^1] : val;
        col.Add(Math.Round(smoothed, decimals));
        if (col.Count > 120) col.RemoveAt(0);
    }

    public override void Dispose()
    {
        _clockTimer.Stop();
        _clockTimer.Dispose();
        _sensorService.ReadingsUpdated -= OnReadingsUpdated;
        base.Dispose();
    }
}
