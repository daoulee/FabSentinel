using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GreenVision.Core;
using GreenVision.Models;
using GreenVision.Services;
using System.Collections.ObjectModel;

namespace GreenVision.ViewModels;

public sealed partial class VisionInspectionViewModel : ViewModelBase
{
    private readonly IVisionInspectionService _visionService;

    [ObservableProperty] private bool _isCameraConnected;
    [ObservableProperty] private bool _isInspecting;
    [ObservableProperty] private string _cameraStatusText = "Camera Disconnected";
    [ObservableProperty] private string _cameraStatusColor = AppConstants.Colors.Danger;

    [ObservableProperty] private string _verdictText = "—";
    [ObservableProperty] private string _verdictColor = AppConstants.Colors.TextSecondary;
    [ObservableProperty] private double _confidence;
    [ObservableProperty] private string _confidenceText = "—";
    [ObservableProperty] private string _elapsedTimeText = "—";
    [ObservableProperty] private int _defectCount;
    [ObservableProperty] private string _modelVersion = "YOLOv8-nano-semiconductor";
    [ObservableProperty] private int _inspectionCount;
    [ObservableProperty] private int _passCount;
    [ObservableProperty] private int _failCount;
    [ObservableProperty] private string _passRate = "—";

    [ObservableProperty] private ObservableCollection<InspectionResult> _history = new();
    [ObservableProperty] private ObservableCollection<DetectedDefect> _currentDefects = new();
    [ObservableProperty] private InspectionResult? _latestResult;

    public VisionInspectionViewModel(IVisionInspectionService visionService)
    {
        _visionService = visionService;
        _visionService.InspectionCompleted += OnInspectionCompleted;
    }

    [RelayCommand]
    private async Task ConnectCameraAsync()
    {
        CameraStatusText = "Connecting...";
        CameraStatusColor = AppConstants.Colors.Warning;
        IsCameraConnected = await _visionService.ConnectCameraAsync();
        if (IsCameraConnected)
        {
            CameraStatusText = "Camera Connected";
            CameraStatusColor = AppConstants.Colors.Safe;
        }
        else
        {
            CameraStatusText = "Connection Failed";
            CameraStatusColor = AppConstants.Colors.Danger;
        }
    }

    [RelayCommand(CanExecute = nameof(CanInspect))]
    private async Task RunInspectionAsync()
    {
        IsInspecting = true;
        VerdictText = "ANALYZING...";
        VerdictColor = AppConstants.Colors.Warning;
        try
        {
            await _visionService.RunInspectionAsync();
        }
        finally
        {
            IsInspecting = false;
        }
    }

    private bool CanInspect() => IsCameraConnected && !IsInspecting;

    private void OnInspectionCompleted(object? sender, InspectionResult result)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            LatestResult = result;
            InspectionCount++;

            if (result.Verdict == InspectionVerdict.Pass) PassCount++;
            else FailCount++;

            PassRate = InspectionCount > 0 ? $"{(double)PassCount / InspectionCount:P1}" : "—";

            VerdictText = result.VerdictText;
            VerdictColor = result.Verdict == InspectionVerdict.Pass
                ? AppConstants.Colors.Safe
                : AppConstants.Colors.Danger;

            Confidence = result.Confidence;
            ConfidenceText = $"{result.Confidence:P1}";
            ElapsedTimeText = $"{result.ElapsedTime.TotalMilliseconds:F0} ms";
            DefectCount = result.Defects.Count;

            CurrentDefects.Clear();
            foreach (var d in result.Defects) CurrentDefects.Add(d);

            History.Insert(0, result);
            if (History.Count > 50) History.RemoveAt(50);
        });
    }

    public override void Dispose()
    {
        _visionService.InspectionCompleted -= OnInspectionCompleted;
        base.Dispose();
    }
}
