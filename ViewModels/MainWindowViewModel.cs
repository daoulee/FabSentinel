using CommunityToolkit.Mvvm.ComponentModel;
using GreenVision.Core;
using GreenVision.Services;
using System.Collections.ObjectModel;

namespace GreenVision.ViewModels;

public sealed partial class NavItemViewModel : ObservableObject
{
    public string LocalizationKey { get; }
    public string Icon { get; }
    public ViewModelBase ViewModel { get; }

    [ObservableProperty] private string _title = string.Empty;
    [ObservableProperty] private bool _isSelected;

    public NavItemViewModel(string locKey, string icon, ViewModelBase viewModel, string initialTitle)
    {
        LocalizationKey = locKey;
        Icon = icon;
        ViewModel = viewModel;
        _title = initialTitle;
    }
}

public sealed partial class MainWindowViewModel : ViewModelBase
{
    private readonly IAlarmService _alarmService;
    private readonly ILocalizationService _loc;

    [ObservableProperty] private ViewModelBase? _currentPage;
    [ObservableProperty] private NavItemViewModel? _selectedNavItem;
    [ObservableProperty] private string _currentDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    [ObservableProperty] private string _alarmColor = AppConstants.Colors.Safe;
    [ObservableProperty] private string _alarmStatus = "SAFE";

    public ObservableCollection<NavItemViewModel> NavItems { get; }

    private readonly System.Threading.Timer _clockTimer;

    public MainWindowViewModel(
        ISensorDataService sensorService,
        IHardwareMonitorService hardwareService,
        IAlarmService alarmService,
        ILocalizationService localizationService,
        DashboardViewModel dashboard,
        FabMonitoringViewModel fabMonitoring,
        VisionInspectionViewModel vision,
        HardwareViewModel hardware,
        LogsViewModel logs,
        SettingsViewModel settings,
        AboutViewModel about)
    {
        _alarmService = alarmService;
        _loc = localizationService;

        NavItems = new ObservableCollection<NavItemViewModel>
        {
            new("nav.dashboard", "ViewDashboard",      dashboard,     _loc["nav.dashboard"]),
            new("nav.fab",       "Factory",            fabMonitoring, _loc["nav.fab"]),
            new("nav.vision",    "Camera",             vision,        _loc["nav.vision"]),
            new("nav.hardware",  "Memory",             hardware,      _loc["nav.hardware"]),
            new("nav.logs",      "FormatListBulleted", logs,          _loc["nav.logs"]),
            new("nav.settings",  "Cog",                settings,      _loc["nav.settings"]),
            new("nav.about",     "InformationOutline", about,         _loc["nav.about"]),
        };

        SelectedNavItem = NavItems[0];

        _loc.LanguageChanged += OnLanguageChanged;

        _alarmService.AlarmLevelChanged += (_, _) =>
        {
            AlarmColor = _alarmService.TowerLampColor;
            AlarmStatus = _loc[_alarmService.TowerLampStatus == "SAFE"    ? "status.safe"
                              : _alarmService.TowerLampStatus == "WARNING" ? "status.warning"
                              : "status.danger"];
        };

        _clockTimer = new System.Threading.Timer(_ =>
        {
            CurrentDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }, null, 0, 1000);

        _ = StartServicesAsync(sensorService, hardwareService);
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        foreach (var item in NavItems)
            item.Title = _loc[item.LocalizationKey];
    }

    partial void OnSelectedNavItemChanged(NavItemViewModel? value)
    {
        if (value is null) return;
        foreach (var item in NavItems) item.IsSelected = item == value;
        CurrentPage = value.ViewModel;
    }

    private static async Task StartServicesAsync(
        ISensorDataService sensorService,
        IHardwareMonitorService hardwareService)
    {
        await sensorService.StartAsync();
        await hardwareService.StartAsync();
    }

    public override void Dispose()
    {
        _loc.LanguageChanged -= OnLanguageChanged;
        _clockTimer.Dispose();
        base.Dispose();
    }
}
