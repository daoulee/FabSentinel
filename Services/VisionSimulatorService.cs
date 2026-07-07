using GreenVision.Helpers;
using GreenVision.Models;

namespace GreenVision.Services;

public sealed class VisionSimulatorService : IVisionInspectionService
{
    private readonly List<InspectionResult> _history = new();
    private readonly ILogService _logService;
    private int _inspectionCounter;

    public IReadOnlyList<InspectionResult> History => _history;
    public bool IsCameraConnected { get; private set; }
    public event EventHandler<InspectionResult>? InspectionCompleted;

    private static readonly string[] DefectLabels = { "Scratch", "Particle", "Bridge", "Void", "Oxidation", "Edge Chip" };

    public VisionSimulatorService(ILogService logService) => _logService = logService;

    public async Task<bool> ConnectCameraAsync(int cameraIndex = 0)
    {
        await Task.Delay(800);
        IsCameraConnected = true;
        _logService.Log(LogCategory.Hardware, LogSeverity.Info, "Camera", $"Camera index {cameraIndex} connected (simulated)");
        return true;
    }

    public async Task<InspectionResult> RunInspectionAsync(CancellationToken ct = default)
    {
        var start = DateTime.Now;
        _logService.Log(LogCategory.Inspection, LogSeverity.Info, "VisionAI", "Inspection started — running YOLO inference (simulated)");

        // Simulate inference time: 200-800ms
        await Task.Delay(SimulationHelper.NextBetween(200, 800) is double d ? (int)d : 400, ct);

        var hasDefect = SimulationHelper.Chance(0.25);
        var defects = new List<DetectedDefect>();

        if (hasDefect)
        {
            int count = (int)SimulationHelper.NextBetween(1, 4);
            for (int i = 0; i < count; i++)
            {
                defects.Add(new DetectedDefect
                {
                    Label = DefectLabels[Random.Shared.Next(DefectLabels.Length)],
                    Confidence = SimulationHelper.NextBetween(0.6, 0.99),
                    X = SimulationHelper.NextBetween(50, 400),
                    Y = SimulationHelper.NextBetween(50, 300),
                    Width = SimulationHelper.NextBetween(20, 80),
                    Height = SimulationHelper.NextBetween(20, 60),
                });
            }
        }

        var result = new InspectionResult
        {
            InspectionNumber = ++_inspectionCounter,
            Verdict = hasDefect ? InspectionVerdict.Fail : InspectionVerdict.Pass,
            Confidence = hasDefect
                ? SimulationHelper.NextBetween(0.72, 0.98)
                : SimulationHelper.NextBetween(0.91, 0.999),
            Defects = defects,
            ElapsedTime = DateTime.Now - start,
            ModelVersion = "YOLOv8-nano-semiconductor"
        };

        _history.Insert(0, result);
        if (_history.Count > 100) _history.RemoveAt(100);

        var verdict = result.Verdict == InspectionVerdict.Pass ? "PASS" : $"FAIL ({result.Defects.Count} defect(s))";
        _logService.Log(
            LogCategory.Inspection,
            result.Verdict == InspectionVerdict.Fail ? LogSeverity.Warning : LogSeverity.Info,
            "VisionAI",
            $"Inspection #{_inspectionCounter}: {verdict} — {result.Confidence:P1} confidence in {result.ElapsedTime.TotalMilliseconds:F0}ms");

        InspectionCompleted?.Invoke(this, result);
        return result;
    }
}
