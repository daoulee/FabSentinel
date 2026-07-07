namespace GreenVision.Models;

public sealed class SensorReading
{
    public string Name { get; init; } = string.Empty;
    public string Unit { get; init; } = string.Empty;
    public double Value { get; set; }
    public double WarningThreshold { get; init; }
    public double DangerThreshold { get; init; }
    public double MinValue { get; init; }
    public double MaxValue { get; init; }
    public DateTime Timestamp { get; set; } = DateTime.Now;

    public AlarmLevel AlarmLevel => Value switch
    {
        var v when v >= DangerThreshold => AlarmLevel.Danger,
        var v when v >= WarningThreshold => AlarmLevel.Warning,
        _ => AlarmLevel.Safe
    };

    public double NormalizedValue =>
        MaxValue > MinValue
            ? Math.Clamp((Value - MinValue) / (MaxValue - MinValue), 0.0, 1.0)
            : 0.0;
}
