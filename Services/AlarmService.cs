using GreenVision.Core;
using GreenVision.Models;

namespace GreenVision.Services;

public sealed class AlarmService : IAlarmService
{
    private readonly ILogService _logService;
    private readonly List<LogEntry> _recentAlarms = new();
    private AlarmLevel _currentLevel = AlarmLevel.Safe;

    public AlarmLevel CurrentSystemAlarm => _currentLevel;
    public IReadOnlyList<LogEntry> RecentAlarms => _recentAlarms;
    public event EventHandler<AlarmLevel>? AlarmLevelChanged;

    public string TowerLampColor => _currentLevel switch
    {
        AlarmLevel.Safe => AppConstants.Colors.Safe,
        AlarmLevel.Warning => AppConstants.Colors.Warning,
        AlarmLevel.Danger => AppConstants.Colors.Danger,
        _ => AppConstants.Colors.Safe
    };

    public string TowerLampStatus => _currentLevel switch
    {
        AlarmLevel.Safe => "SAFE",
        AlarmLevel.Warning => "WARNING",
        AlarmLevel.Danger => "DANGER",
        _ => "SAFE"
    };

    public AlarmService(ILogService logService) => _logService = logService;

    public void ProcessReadings(IReadOnlyList<SensorReading> readings)
    {
        var maxLevel = AlarmLevel.Safe;
        foreach (var reading in readings)
        {
            if (reading.AlarmLevel > maxLevel)
                maxLevel = reading.AlarmLevel;

            if (reading.AlarmLevel > AlarmLevel.Safe)
            {
                var severity = reading.AlarmLevel == AlarmLevel.Danger
                    ? LogSeverity.Error
                    : LogSeverity.Warning;

                var entry = new LogEntry
                {
                    Category = LogCategory.Alarm,
                    Severity = severity,
                    Source = reading.Name,
                    Message = $"{reading.Name} {reading.AlarmLevel.ToString().ToUpper()}: {reading.Value:F1} {reading.Unit} (threshold: {reading.WarningThreshold})"
                };

                if (_recentAlarms.Count == 0 ||
                    (DateTime.Now - _recentAlarms[^1].Timestamp).TotalSeconds > 10 ||
                    _recentAlarms[^1].Source != reading.Name)
                {
                    _recentAlarms.Insert(0, entry);
                    if (_recentAlarms.Count > 50) _recentAlarms.RemoveAt(50);
                    _logService.Log(LogCategory.Alarm, severity, reading.Name, entry.Message);
                }
            }
        }

        if (maxLevel != _currentLevel)
        {
            _currentLevel = maxLevel;
            AlarmLevelChanged?.Invoke(this, _currentLevel);
        }
    }
}
