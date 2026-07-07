namespace GreenVision.Models;

public enum InspectionVerdict { Pass, Fail, Pending }

public sealed class DetectedDefect
{
    public string Label { get; init; } = string.Empty;
    public double Confidence { get; init; }
    public double X { get; init; }
    public double Y { get; init; }
    public double Width { get; init; }
    public double Height { get; init; }
}

public sealed class InspectionResult
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; init; } = DateTime.Now;
    public InspectionVerdict Verdict { get; set; } = InspectionVerdict.Pending;
    public double Confidence { get; set; }
    public List<DetectedDefect> Defects { get; init; } = new();
    public TimeSpan ElapsedTime { get; set; }
    public string ImagePath { get; set; } = string.Empty;
    public string ModelVersion { get; set; } = "YOLOv8-nano";
    public int InspectionNumber { get; set; }

    public string VerdictText => Verdict switch
    {
        InspectionVerdict.Pass => "PASS",
        InspectionVerdict.Fail => "FAIL",
        InspectionVerdict.Pending => "PENDING",
        _ => "UNKNOWN"
    };
}
