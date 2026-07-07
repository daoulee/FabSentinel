using GreenVision.Models;

namespace GreenVision.Services;

public interface IAlarmService
{
    AlarmLevel CurrentSystemAlarm { get; }
    string TowerLampColor { get; }
    string TowerLampStatus { get; }
    IReadOnlyList<LogEntry> RecentAlarms { get; }
    event EventHandler<AlarmLevel>? AlarmLevelChanged;
    void ProcessReadings(IReadOnlyList<SensorReading> readings);
}
