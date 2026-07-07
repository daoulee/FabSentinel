namespace GreenVision.Models;

public sealed class AppSettings
{
    public string Theme { get; set; } = "Dark";
    public string Language { get; set; } = "en-US";
    public string ComPort { get; set; } = "COM3";
    public string ApiBaseUrl { get; set; } = "http://localhost:8000";
    public int CameraIndex { get; set; } = 0;
    public int SensorUpdateIntervalMs { get; set; } = 1000;
    public bool EnableAlarmSound { get; set; } = true;
    public bool EnableNotifications { get; set; } = true;
    public string DatabasePath { get; set; } = "greenvision.db";
    public ThresholdSettings Thresholds { get; set; } = new();
}

public sealed class ThresholdSettings
{
    public double Pm25Warning { get; set; } = 35.0;
    public double Pm25Danger { get; set; } = 75.0;
    public double Pm10Warning { get; set; } = 50.0;
    public double Pm10Danger { get; set; } = 150.0;
    public double Nh3Warning { get; set; } = 25.0;
    public double Nh3Danger { get; set; } = 50.0;
    public double CoWarning { get; set; } = 9.0;
    public double CoDanger { get; set; } = 35.0;
    public double TempWarning { get; set; } = 28.0;
    public double TempDanger { get; set; } = 35.0;
    public double HumidityWarning { get; set; } = 65.0;
    public double HumidityDanger { get; set; } = 80.0;
}
