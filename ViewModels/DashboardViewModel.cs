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

public record SensorStats(string Avg, string Min, string Max);

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

    private readonly ObservableCollection<double> _pm25Values = new();
    private readonly ObservableCollection<double> _pm10Values = new();
    private readonly ObservableCollection<double> _nh3Values  = new();
    private readonly ObservableCollection<double> _coValues   = new();
    private readonly ObservableCollection<double> _tempValues = new();
    private readonly ObservableCollection<double> _humidValues = new();

    public ISeries[] Pm25Series  { get; }
    public ISeries[] Pm10Series  { get; }
    public ISeries[] Nh3Series   { get; }
    public ISeries[] CoSeries    { get; }
    public ISeries[] TempSeries  { get; }
    public ISeries[] HumidSeries { get; }

    public Axis[] XAxes      { get; } = new Axis[] { new Axis { IsVisible = false, ShowSeparatorLines = false } };
    public Axis[] YAxesPm25  { get; }
    public Axis[] YAxesPm10  { get; }
    public Axis[] YAxesNh3   { get; }
    public Axis[] YAxesCo    { get; }
    public Axis[] YAxesTemp  { get; }
    public Axis[] YAxesHumid { get; }

    [ObservableProperty] private SensorStats _pm25Stats  = new("-", "-", "-");
    [ObservableProperty] private SensorStats _pm10Stats  = new("-", "-", "-");
    [ObservableProperty] private SensorStats _nh3Stats   = new("-", "-", "-");
    [ObservableProperty] private SensorStats _coStats    = new("-", "-", "-");
    [ObservableProperty] private SensorStats _tempStats  = new("-", "-", "-");
    [ObservableProperty] private SensorStats _humidStats = new("-", "-", "-");

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

        var rng = Random.Shared;
        for (int i = 0; i < 40; i++)
        {
            _pm25Values.Add(Math.Round(13 + rng.NextDouble() * 4, 1));
            _pm10Values.Add(Math.Round(28 + rng.NextDouble() * 6, 1));
            _nh3Values.Add(Math.Round(4.5 + rng.NextDouble() * 1.5, 1));
            _coValues.Add(Math.Round(2.5 + rng.NextDouble() * 1.0, 2));
            _tempValues.Add(Math.Round(22 + rng.NextDouble() * 1.5, 1));
            _humidValues.Add(Math.Round(54 + rng.NextDouble() * 3, 1));
        }

        var axisPaint = new SolidColorPaint(new SKColor(0x7D, 0x85, 0x90));
        var sepPaint  = new SolidColorPaint(new SKColor(0x1E, 0x27, 0x33));

        Pm25Series  = MakeSeries(_pm25Values,  new SKColor(0xEF, 0x44, 0x44, 80),  new SKColor(0xEF, 0x44, 0x44));
        Pm10Series  = MakeSeries(_pm10Values,  new SKColor(0xF9, 0x73, 0x16, 70),  new SKColor(0xF9, 0x73, 0x16));
        Nh3Series   = MakeSeries(_nh3Values,   new SKColor(0xA8, 0x55, 0xF7, 70),  new SKColor(0xA8, 0x55, 0xF7));
        CoSeries    = MakeSeries(_coValues,    new SKColor(0xF5, 0x9E, 0x0B, 70),  new SKColor(0xF5, 0x9E, 0x0B));
        TempSeries  = MakeSeries(_tempValues,  new SKColor(0x00, 0xD2, 0xB5, 80),  new SKColor(0x00, 0xD2, 0xB5));
        HumidSeries = MakeSeries(_humidValues, new SKColor(0x88, 0x55, 0xFF, 70),  new SKColor(0x88, 0x55, 0xFF));

        YAxesPm25  = MakeAxis(axisPaint, sepPaint, minLimit: 0,  maxLimit: 50);
        YAxesPm10  = MakeAxis(axisPaint, sepPaint, minLimit: 0,  maxLimit: 80);
        YAxesNh3   = MakeAxis(axisPaint, sepPaint, minLimit: 0,  maxLimit: 20);
        YAxesCo    = MakeAxis(axisPaint, sepPaint, minLimit: 0,  maxLimit: 8);
        YAxesTemp  = MakeAxis(axisPaint, sepPaint, minLimit: 18, maxLimit: 30);
        YAxesHumid = MakeAxis(axisPaint, sepPaint, minLimit: 40, maxLimit: 75);

        _sensorService.ReadingsUpdated += OnReadingsUpdated;
        _alarmService.AlarmLevelChanged += OnAlarmLevelChanged;
    }

    private static ISeries[] MakeSeries(ObservableCollection<double> values, SKColor fillColor, SKColor strokeColor)
    {
        var fill = new LinearGradientPaint(
            new[] { fillColor, new SKColor(fillColor.Red, fillColor.Green, fillColor.Blue, 0) },
            new SKPoint(0.5f, 0f), new SKPoint(0.5f, 1f));

        return new ISeries[]
        {
            new LineSeries<double>
            {
                Values = values,
                Fill = fill,
                Stroke = new SolidColorPaint(strokeColor) { StrokeThickness = 2 },
                GeometrySize = 0, GeometryFill = null, GeometryStroke = null,
            }
        };
    }

    private static Axis[] MakeAxis(SolidColorPaint labelPaint, SolidColorPaint sepPaint,
                                   double minLimit, double maxLimit)
    {
        return new Axis[]
        {
            new Axis
            {
                LabelsPaint = labelPaint,
                SeparatorsPaint = sepPaint,
                TicksPaint = null,
                MinLimit = minLimit,
                MaxLimit = maxLimit,
            }
        };
    }

    private void OnReadingsUpdated(object? sender, IReadOnlyList<SensorReading> readings)
    {
        Dispatcher.UIThread.Post(() =>
        {
            foreach (var r in readings)
            {
                switch (r.Name)
                {
                    case "PM2.5":        Pm25Card.Update(r);      Push(_pm25Values,  r.Value, 1); Pm25Stats  = Stats(_pm25Values,  1); break;
                    case "PM10":         Pm10Card.Update(r);      Push(_pm10Values,  r.Value, 1); Pm10Stats  = Stats(_pm10Values,  1); break;
                    case "NH3":          Nh3Card.Update(r);       Push(_nh3Values,   r.Value, 1); Nh3Stats   = Stats(_nh3Values,   1); break;
                    case "CO":           CoCard.Update(r);        Push(_coValues,    r.Value, 2); CoStats    = Stats(_coValues,    2); break;
                    case "Temperature":  TempCard.Update(r);      Push(_tempValues,  r.Value, 1); TempStats  = Stats(_tempValues,  1); break;
                    case "Humidity":     HumidityCard.Update(r);  Push(_humidValues, r.Value, 1); HumidStats = Stats(_humidValues, 1); break;
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

    private static SensorStats Stats(ObservableCollection<double> col, int decimals)
    {
        if (col.Count == 0) return new SensorStats("-", "-", "-");
        string fmt = $"F{decimals}";
        return new SensorStats(
            col.Average().ToString(fmt),
            col.Min().ToString(fmt),
            col.Max().ToString(fmt));
    }

    private static void Push(ObservableCollection<double> col, double value, int decimals = 1)
    {
        const double alpha = 0.2;
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
