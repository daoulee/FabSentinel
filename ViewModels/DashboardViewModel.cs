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

public sealed partial class DashboardViewModel : ViewModelBase
{
    private readonly ISensorDataService _sensorService;
    private readonly IAlarmService _alarmService;

    public SensorCardViewModel Pm25Card { get; }
    public SensorCardViewModel Pm10Card { get; }
    public SensorCardViewModel Nh3Card { get; }
    public SensorCardViewModel CoCard { get; }
    public SensorCardViewModel TempCard { get; }
    public SensorCardViewModel HumidityCard { get; }

    private readonly ObservableCollection<double> _tempValues = new();
    private readonly ObservableCollection<double> _humidValues = new();
    private readonly ObservableCollection<double> _pm25Values = new();
    private readonly ObservableCollection<double> _coValues = new();

    public ISeries[] TempHumiditySeries { get; }
    public ISeries[] AirQualitySeries { get; }
    public Axis[] XAxes { get; } = new Axis[] { new Axis { IsVisible = false, ShowSeparatorLines = false } };
    public Axis[] YAxesTemp { get; }
    public Axis[] YAxesAir { get; }

    [ObservableProperty] private string _towerLampStatus = "SAFE";
    [ObservableProperty] private string _towerLampColor = AppConstants.Colors.Safe;

    public string TowerLampIcon => TowerLampStatus switch
    {
        "WARNING" => "AlertOutline",
        "DANGER"  => "AlertOctagonOutline",
        _         => "ShieldCheckOutline"
    };

    partial void OnTowerLampStatusChanged(string value) =>
        OnPropertyChanged(nameof(TowerLampIcon));
    [ObservableProperty] private ObservableCollection<LogEntry> _recentAlarms = new();

    // ── Tab state ─────────────────────────────────────────────────────────
    [ObservableProperty] private string _selectedTab = "Overview";
    public bool IsOverviewTab => SelectedTab == "Overview";
    public bool IsSensorsTab  => SelectedTab == "Sensors";
    public bool IsAlarmsTab   => SelectedTab == "Alarms";

    public SensorCardViewModel[] AllSensors { get; private set; } = [];

    [RelayCommand]
    private void SelectTab(string tab) => SelectedTab = tab;

    partial void OnSelectedTabChanged(string value)
    {
        OnPropertyChanged(nameof(IsOverviewTab));
        OnPropertyChanged(nameof(IsSensorsTab));
        OnPropertyChanged(nameof(IsAlarmsTab));
    }

    public DashboardViewModel(ISensorDataService sensorService, IAlarmService alarmService)
    {
        _sensorService = sensorService;
        _alarmService = alarmService;

        Pm25Card     = new SensorCardViewModel("PM2.5",       "μg/m³", AppConstants.Thresholds.Pm25Warning,     AppConstants.Thresholds.Pm25Danger,     200, "Grain");
        Pm10Card     = new SensorCardViewModel("PM10",        "μg/m³", AppConstants.Thresholds.Pm10Warning,     AppConstants.Thresholds.Pm10Danger,     300, "Blur");
        Nh3Card      = new SensorCardViewModel("NH3",         "ppm",   AppConstants.Thresholds.Nh3Warning,      AppConstants.Thresholds.Nh3Danger,      100, "Flask");
        CoCard       = new SensorCardViewModel("CO",          "ppm",   AppConstants.Thresholds.CoWarning,       AppConstants.Thresholds.CoDanger,       50,  "Smoke");
        TempCard     = new SensorCardViewModel("Temperature", "°C",    AppConstants.Thresholds.TempWarning,     AppConstants.Thresholds.TempDanger,     60,  "Thermometer");
        HumidityCard = new SensorCardViewModel("Humidity",   "%",     AppConstants.Thresholds.HumidityWarning, AppConstants.Thresholds.HumidityDanger, 100, "WaterPercent");
        AllSensors   = new[] { Pm25Card, Pm10Card, Nh3Card, CoCard, TempCard, HumidityCard };

        // Seed initial data (rounded to 1dp so tooltip shows clean values)
        var rng = Random.Shared;
        for (int i = 0; i < 40; i++)
        {
            _tempValues.Add(Math.Round(22 + rng.NextDouble() * 4, 1));
            _humidValues.Add(Math.Round(52 + rng.NextDouble() * 8, 1));
            _pm25Values.Add(Math.Round(12 + rng.NextDouble() * 10, 1));
            _coValues.Add(Math.Round(2.5 + rng.NextDouble() * 2, 2));
        }

        // Teal gradient for temperature
        var tealGrad = new LinearGradientPaint(
            new SKColor[] { new SKColor(0, 210, 181, 90), new SKColor(0, 210, 181, 0) },
            new SKPoint(0.5f, 0f), new SKPoint(0.5f, 1f));

        // Purple gradient for humidity
        var purpleGrad = new LinearGradientPaint(
            new SKColor[] { new SKColor(136, 85, 255, 70), new SKColor(136, 85, 255, 0) },
            new SKPoint(0.5f, 0f), new SKPoint(0.5f, 1f));

        // Red gradient for PM2.5
        var redGrad = new LinearGradientPaint(
            new SKColor[] { new SKColor(239, 68, 68, 80), new SKColor(239, 68, 68, 0) },
            new SKPoint(0.5f, 0f), new SKPoint(0.5f, 1f));

        // Orange gradient for CO
        var orangeGrad = new LinearGradientPaint(
            new SKColor[] { new SKColor(245, 158, 11, 70), new SKColor(245, 158, 11, 0) },
            new SKPoint(0.5f, 0f), new SKPoint(0.5f, 1f));

        TempHumiditySeries = new ISeries[]
        {
            new LineSeries<double>
            {
                Name = "Temperature (°C)",
                Values = _tempValues,
                Fill = tealGrad,
                Stroke = new SolidColorPaint(new SKColor(0x00, 0xD2, 0xB5)) { StrokeThickness = 2 },
                GeometrySize = 0, GeometryFill = null, GeometryStroke = null,
            },
            new LineSeries<double>
            {
                Name = "Humidity (%)",
                Values = _humidValues,
                Fill = purpleGrad,
                Stroke = new SolidColorPaint(new SKColor(0x88, 0x55, 0xFF)) { StrokeThickness = 2 },
                GeometrySize = 0, GeometryFill = null, GeometryStroke = null,
            }
        };

        AirQualitySeries = new ISeries[]
        {
            new LineSeries<double>
            {
                Name = "PM2.5 (μg/m³)",
                Values = _pm25Values,
                Fill = redGrad,
                Stroke = new SolidColorPaint(new SKColor(0xEF, 0x44, 0x44)) { StrokeThickness = 2 },
                GeometrySize = 0, GeometryFill = null, GeometryStroke = null,
            },
            new LineSeries<double>
            {
                Name = "CO (ppm)",
                Values = _coValues,
                Fill = orangeGrad,
                Stroke = new SolidColorPaint(new SKColor(0xF5, 0x9E, 0x0B)) { StrokeThickness = 2 },
                GeometrySize = 0, GeometryFill = null, GeometryStroke = null,
            }
        };

        var axisPaint = new SolidColorPaint(new SKColor(0x7D, 0x85, 0x90));
        var sepPaint = new SolidColorPaint(new SKColor(0x1E, 0x27, 0x33));
        YAxesTemp = new Axis[] { new Axis { LabelsPaint = axisPaint, SeparatorsPaint = sepPaint, TicksPaint = null } };
        YAxesAir = new Axis[] { new Axis { LabelsPaint = axisPaint, SeparatorsPaint = sepPaint, TicksPaint = null } };

        _sensorService.ReadingsUpdated += OnReadingsUpdated;
        _alarmService.AlarmLevelChanged += OnAlarmLevelChanged;
    }

    private void OnReadingsUpdated(object? sender, IReadOnlyList<SensorReading> readings)
    {
        Dispatcher.UIThread.Post(() =>
        {
            foreach (var r in readings)
            {
                switch (r.Name)
                {
                    case "PM2.5": Pm25Card.Update(r); Push(_pm25Values, r.Value, 1); break;
                    case "PM10": Pm10Card.Update(r); break;
                    case "NH3": Nh3Card.Update(r); break;
                    case "CO": CoCard.Update(r); Push(_coValues, r.Value, 2); break;
                    case "Temperature": TempCard.Update(r); Push(_tempValues, r.Value, 1); break;
                    case "Humidity": HumidityCard.Update(r); Push(_humidValues, r.Value, 1); break;
                }
            }
            _alarmService.ProcessReadings(readings);

            var alarms = _alarmService.RecentAlarms.Take(8).ToList();
            RecentAlarms.Clear();
            foreach (var a in alarms) RecentAlarms.Add(a);
        });
    }

    private void OnAlarmLevelChanged(object? sender, AlarmLevel level)
    {
        Dispatcher.UIThread.Post(() =>
        {
            TowerLampColor = _alarmService.TowerLampColor;
            TowerLampStatus = _alarmService.TowerLampStatus;
        });
    }

    private static void Push(ObservableCollection<double> col, double value, int decimals = 1)
    {
        const double alpha = 0.25; // EMA: 25% new value, 75% history → smooth curve
        double smoothed = col.Count > 0 ? alpha * value + (1 - alpha) * col[^1] : value;
        col.Add(Math.Round(smoothed, decimals));
        if (col.Count > AppConstants.Simulation.ChartWindowSize) col.RemoveAt(0);
    }

    public override void Dispose()
    {
        _sensorService.ReadingsUpdated -= OnReadingsUpdated;
        _alarmService.AlarmLevelChanged -= OnAlarmLevelChanged;
        base.Dispose();
    }
}
