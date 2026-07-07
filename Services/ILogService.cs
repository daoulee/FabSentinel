using GreenVision.Models;

namespace GreenVision.Services;

public interface ILogService
{
    IReadOnlyList<LogEntry> Entries { get; }
    event EventHandler<LogEntry>? EntryAdded;
    void Log(LogCategory category, LogSeverity severity, string source, string message, string? details = null);
    void Info(string source, string message) => Log(LogCategory.System, LogSeverity.Info, source, message);
    void Warn(string source, string message) => Log(LogCategory.System, LogSeverity.Warning, source, message);
    void Error(string source, string message, string? details = null) =>
        Log(LogCategory.System, LogSeverity.Error, source, message, details);
    IReadOnlyList<LogEntry> GetFiltered(LogCategory? category = null, LogSeverity? minSeverity = null, string? search = null);
}
