using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GreenVision.Core;
using GreenVision.Models;
using GreenVision.Services;
using System.Collections.ObjectModel;
using System.Text;

namespace GreenVision.ViewModels;

public sealed partial class LogsViewModel : ViewModelBase
{
    private readonly ILogService _logService;

    [ObservableProperty] private ObservableCollection<LogEntry> _displayedEntries = new();
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private string _selectedCategory = "All";
    [ObservableProperty] private string _selectedSeverity = "All";
    [ObservableProperty] private int _totalCount;
    [ObservableProperty] private int _errorCount;
    [ObservableProperty] private int _warnCount;

    public string[] Categories { get; } = { "All", "System", "Sensor", "Alarm", "Inspection", "Hardware", "Communication" };
    public string[] Severities { get; } = { "All", "Info", "Warning", "Error", "Critical" };

    public LogsViewModel(ILogService logService)
    {
        _logService = logService;
        _logService.EntryAdded += (_, _) => Avalonia.Threading.Dispatcher.UIThread.Post(RefreshDisplay);
        RefreshDisplay();
    }

    partial void OnSearchTextChanged(string value) => RefreshDisplay();
    partial void OnSelectedCategoryChanged(string value) => RefreshDisplay();
    partial void OnSelectedSeverityChanged(string value) => RefreshDisplay();

    private void RefreshDisplay()
    {
        LogCategory? cat = SelectedCategory == "All" ? null : Enum.Parse<LogCategory>(SelectedCategory);
        LogSeverity? sev = SelectedSeverity == "All" ? null : Enum.Parse<LogSeverity>(SelectedSeverity);
        var filtered = _logService.GetFiltered(cat, sev, string.IsNullOrWhiteSpace(SearchText) ? null : SearchText);

        DisplayedEntries.Clear();
        foreach (var e in filtered.Take(200)) DisplayedEntries.Add(e);

        var all = _logService.Entries;
        TotalCount = all.Count;
        ErrorCount = all.Count(e => e.Severity >= LogSeverity.Error);
        WarnCount = all.Count(e => e.Severity == LogSeverity.Warning);
    }

    [RelayCommand]
    private async Task ExportCsvAsync()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Timestamp,Category,Severity,Source,Message");
        foreach (var e in _logService.Entries)
            sb.AppendLine($"{e.FormattedTimestamp},{e.Category},{e.SeverityText},{e.Source},\"{e.Message}\"");

        var path = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            $"greenvision_logs_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        await File.WriteAllTextAsync(path, sb.ToString());
        _logService.Info("LogExport", $"Logs exported to {path}");
    }

    [RelayCommand]
    private void ClearFilter()
    {
        SearchText = string.Empty;
        SelectedCategory = "All";
        SelectedSeverity = "All";
    }
}
