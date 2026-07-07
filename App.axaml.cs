using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using GreenVision.Services;
using GreenVision.ViewModels;
using GreenVision.Views;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Microsoft.Extensions.DependencyInjection;

namespace GreenVision;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        LiveCharts.Configure(config =>
            config.AddSkiaSharp().AddDefaultMappers().AddDarkTheme());

        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();

        // 센서 데이터 → Supabase 동기화 시작
        Services.GetRequiredService<SensorDataSyncService>();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = Services.GetRequiredService<MainWindowViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Infrastructure
        services.AddSingleton<ILogService, LogService>();
        services.AddSingleton<ILocalizationService, LocalizationService>();
        services.AddSingleton<SensorDataSyncService>();

        // Services
        services.AddSingleton<ISensorDataService, SensorSimulatorService>();
        services.AddSingleton<IHardwareMonitorService, HardwareSimulatorService>();
        services.AddSingleton<IVisionInspectionService, VisionSimulatorService>();
        services.AddSingleton<IAlarmService, AlarmService>();

        // Page ViewModels
        services.AddSingleton<DashboardViewModel>();
        services.AddSingleton<FabMonitoringViewModel>();
        services.AddSingleton<VisionInspectionViewModel>();
        services.AddSingleton<HardwareViewModel>();
        services.AddSingleton<LogsViewModel>();
        services.AddSingleton<SettingsViewModel>();
        services.AddSingleton<AboutViewModel>();

        // Shell
        services.AddSingleton<MainWindowViewModel>();
    }
}
