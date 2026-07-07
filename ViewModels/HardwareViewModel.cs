using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using GreenVision.Core;
using GreenVision.Models;
using GreenVision.Services;
using System.Collections.ObjectModel;

namespace GreenVision.ViewModels;

public sealed partial class HardwareViewModel : ViewModelBase
{
    private readonly IHardwareMonitorService _hardwareService;

    [ObservableProperty] private ObservableCollection<HardwareCardViewModel> _deviceCards = new();
    [ObservableProperty] private int _onlineCount;
    [ObservableProperty] private int _totalCount;
    [ObservableProperty] private string _systemHealthText = "All Systems Operational";
    [ObservableProperty] private string _systemHealthColor = AppConstants.Colors.Safe;

    public HardwareViewModel(IHardwareMonitorService hardwareService)
    {
        _hardwareService = hardwareService;
        _hardwareService.DevicesUpdated += OnDevicesUpdated;

        foreach (var device in _hardwareService.Devices)
            DeviceCards.Add(new HardwareCardViewModel(device));

        UpdateSummary();
    }

    private void OnDevicesUpdated(object? sender, IReadOnlyList<HardwareDevice> devices)
    {
        Dispatcher.UIThread.Post(() =>
        {
            // Update existing cards or add new ones
            foreach (var device in devices)
            {
                var card = DeviceCards.FirstOrDefault(c => c.Name == device.Name);
                if (card is not null) card.Update(device);
                else DeviceCards.Add(new HardwareCardViewModel(device));
            }
            UpdateSummary();
        });
    }

    private void UpdateSummary()
    {
        TotalCount = DeviceCards.Count;
        OnlineCount = DeviceCards.Count(c => c.Status == DeviceStatus.Online);

        (SystemHealthText, SystemHealthColor) = OnlineCount == TotalCount
            ? ("All Systems Operational", AppConstants.Colors.Safe)
            : OnlineCount >= TotalCount - 1
            ? ("Minor Connectivity Issue", AppConstants.Colors.Warning)
            : ("Multiple Devices Offline", AppConstants.Colors.Danger);
    }

    public override void Dispose()
    {
        _hardwareService.DevicesUpdated -= OnDevicesUpdated;
        base.Dispose();
    }
}
