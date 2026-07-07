namespace GreenVision.Models;

public enum LogCategory { System, Sensor, Alarm, Inspection, Hardware, Communication }
public enum LogSeverity { Info, Warning, Error, Critical }

public sealed class LogEntry
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; init; } = DateTime.Now;
    public LogCategory Category { get; init; }
    public LogSeverity Severity { get; init; }
    public string Source { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string? Details { get; init; }

    public string FormattedTimestamp => Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff");

    public string SeverityText => Severity switch
    {
        LogSeverity.Info => "INFO",
        LogSeverity.Warning => "WARN",
        LogSeverity.Error => "ERROR",
        LogSeverity.Critical => "CRIT",
        _ => "INFO"
    };
}
