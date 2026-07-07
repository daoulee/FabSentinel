using GreenVision.Core;
using GreenVision.Models;
using System.Collections.Concurrent;

namespace GreenVision.Services;

public sealed class LogService : ILogService
{
    private readonly ConcurrentBag<LogEntry> _entries = new();

    public IReadOnlyList<LogEntry> Entries =>
        _entries.OrderByDescending(e => e.Timestamp).Take(AppConstants.Simulation.MaxLogEntries).ToList();

    public event EventHandler<LogEntry>? EntryAdded;

    public LogService()
    {
        Log(LogCategory.System, LogSeverity.Info, "GreenVision", $"System started — v{AppConstants.AppVersion}");
        Log(LogCategory.System, LogSeverity.Info, "SensorService", "Sensor simulation initialized");
        Log(LogCategory.Hardware, LogSeverity.Info, "ESP32", "ESP32 connection established (simulated)");
        Log(LogCategory.Hardware, LogSeverity.Info, "Camera", "USB camera detected at index 0 (simulated)");
        Log(LogCategory.System, LogSeverity.Info, "Database", "SQLite database ready");
    }

    public void Log(LogCategory category, LogSeverity severity, string source, string message, string? details = null)
    {
        var entry = new LogEntry
        {
            Category = category,
            Severity = severity,
            Source = source,
            Message = message,
            Details = details
        };
        _entries.Add(entry);
        EntryAdded?.Invoke(this, entry);
    }

    public IReadOnlyList<LogEntry> GetFiltered(LogCategory? category = null, LogSeverity? minSeverity = null, string? search = null)
    {
        IEnumerable<LogEntry> result = Entries;
        if (category.HasValue) result = result.Where(e => e.Category == category.Value);
        if (minSeverity.HasValue) result = result.Where(e => e.Severity >= minSeverity.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLowerInvariant();
            result = result.Where(e =>
                e.Message.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                e.Source.Contains(s, StringComparison.OrdinalIgnoreCase));
        }
        return result.ToList();
    }
}
