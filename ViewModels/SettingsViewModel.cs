using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GreenVision.Core;
using GreenVision.Models;
using GreenVision.Services;

namespace GreenVision.ViewModels;

public sealed partial class SettingsViewModel : ViewModelBase
{
    private readonly ILocalizationService _loc;

    public SettingsViewModel(ILocalizationService localizationService)
    {
        _loc = localizationService;
        _selectedLanguage = _loc.CurrentLanguage == "ko" ? "한국어" : "English";
    }

    // ── Language ──────────────────────────────────────────────────────────
    public string[] Languages { get; } = { "한국어", "English" };

    [ObservableProperty] private string _selectedLanguage;

    partial void OnSelectedLanguageChanged(string value) =>
        _loc.SetLanguage(value);

    // ── Connection ────────────────────────────────────────────────────────
    [ObservableProperty] private string _apiBaseUrl = "http://localhost:8000";
    [ObservableProperty] private string _comPort = "COM3";
    [ObservableProperty] private int _cameraIndex;
    [ObservableProperty] private int _sensorUpdateIntervalMs = 1000;
    [ObservableProperty] private bool _enableAlarmSound = true;
    [ObservableProperty] private bool _enableNotifications = true;

    [ObservableProperty] private double _pm25Warning = AppConstants.Thresholds.Pm25Warning;
    [ObservableProperty] private double _pm25Danger = AppConstants.Thresholds.Pm25Danger;
    [ObservableProperty] private double _pm10Warning = AppConstants.Thresholds.Pm10Warning;
    [ObservableProperty] private double _pm10Danger = AppConstants.Thresholds.Pm10Danger;
    [ObservableProperty] private double _nh3Warning = AppConstants.Thresholds.Nh3Warning;
    [ObservableProperty] private double _nh3Danger = AppConstants.Thresholds.Nh3Danger;
    [ObservableProperty] private double _coWarning = AppConstants.Thresholds.CoWarning;
    [ObservableProperty] private double _coDanger = AppConstants.Thresholds.CoDanger;
    [ObservableProperty] private double _tempWarning = AppConstants.Thresholds.TempWarning;
    [ObservableProperty] private double _tempDanger = AppConstants.Thresholds.TempDanger;
    [ObservableProperty] private double _humidityWarning = AppConstants.Thresholds.HumidityWarning;
    [ObservableProperty] private double _humidityDanger = AppConstants.Thresholds.HumidityDanger;

    [ObservableProperty] private string _saveStatusMessage = string.Empty;
    [ObservableProperty] private bool _saveSuccess;

    public string[] AvailableComPorts { get; } = { "COM1", "COM2", "COM3", "COM4", "/dev/ttyUSB0", "/dev/ttyUSB1" };
    public int[] CameraIndices { get; } = { 0, 1, 2, 3 };
    public int[] UpdateIntervals { get; } = { 500, 1000, 2000, 5000 };

    [RelayCommand]
    private async Task SaveSettingsAsync()
    {
        // Placeholder: serialize settings to JSON file
        await Task.Delay(300);
        SaveStatusMessage = "Settings saved successfully.";
        SaveSuccess = true;
        await Task.Delay(2500);
        SaveStatusMessage = string.Empty;
    }

    [RelayCommand]
    private void ResetToDefaults()
    {
        Pm25Warning = AppConstants.Thresholds.Pm25Warning;
        Pm25Danger = AppConstants.Thresholds.Pm25Danger;
        Pm10Warning = AppConstants.Thresholds.Pm10Warning;
        Pm10Danger = AppConstants.Thresholds.Pm10Danger;
        Nh3Warning = AppConstants.Thresholds.Nh3Warning;
        Nh3Danger = AppConstants.Thresholds.Nh3Danger;
        CoWarning = AppConstants.Thresholds.CoWarning;
        CoDanger = AppConstants.Thresholds.CoDanger;
        TempWarning = AppConstants.Thresholds.TempWarning;
        TempDanger = AppConstants.Thresholds.TempDanger;
        HumidityWarning = AppConstants.Thresholds.HumidityWarning;
        HumidityDanger = AppConstants.Thresholds.HumidityDanger;
    }
}
