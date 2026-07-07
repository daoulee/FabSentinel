using GreenVision.Core;

namespace GreenVision.ViewModels;

public sealed class AboutViewModel : ViewModelBase
{
    public string AppName => AppConstants.AppName;
    public string AppVersion => $"v{AppConstants.AppVersion}";
    public string AppSubtitle => AppConstants.AppSubtitle;
    public string BuildDate => DateTime.Now.ToString("yyyy-MM-dd");
    public string DotNetVersion => System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription;

    public (string Name, string Role)[] TeamMembers { get; } =
    {
        ("FabSentinel Team", "Capstone Project"),
        ("AI/ML Engineer", "YOLO v8 · ONNX Runtime"),
        ("Embedded Engineer", "ESP32 · Raspberry Pi"),
        ("Software Engineer", "C# · Avalonia UI · MVVM"),
        ("Hardware Engineer", "Sensor Array · Tower Lamp"),
    };

    public (string Name, string Version)[] TechStack { get; } =
    {
        ("C# / .NET 10", "10.0"),
        ("Avalonia UI", "11.2.3"),
        ("CommunityToolkit.Mvvm", "8.3.2"),
        ("LiveChartsCore", "2.0.0-rc4"),
        ("SQLite", "8.0"),
        ("ONNX Runtime", "Planned"),
        ("OpenCV", "Planned"),
    };
}
