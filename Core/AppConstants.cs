namespace GreenVision.Core;

public static class AppConstants
{
    public const string AppName = "FabSentinel";
    public const string AppVersion = "1.0.0";
    public const string AppSubtitle = "AI-Powered Fab Environment Safety Intelligence System";

    public static class Colors
    {
        public const string Primary = "#00D2B5";
        public const string Background = "#0F172A";
        public const string CardBackground = "#1E293B";
        public const string CardSecondary = "#162032";
        public const string TextPrimary = "#F1F5F9";
        public const string TextSecondary = "#94A3B8";
        public const string Safe = "#10B981";
        public const string Warning = "#F59E0B";
        public const string Danger = "#EF4444";
        public const string Border = "#334155";
        public const string Sidebar = "#0B1120";
    }

    public static class Sensors
    {
        public const string Pm25 = "PM2.5";
        public const string Pm10 = "PM10";
        public const string Nh3 = "NH3";
        public const string Co = "CO";
        public const string Temperature = "Temperature";
        public const string Humidity = "Humidity";
    }

    public static class Thresholds
    {
        public const double Pm25Warning = 35.0;
        public const double Pm25Danger = 75.0;
        public const double Pm10Warning = 50.0;
        public const double Pm10Danger = 150.0;
        public const double Nh3Warning = 25.0;
        public const double Nh3Danger = 50.0;
        public const double CoWarning = 9.0;
        public const double CoDanger = 35.0;
        public const double TempWarning = 28.0;
        public const double TempDanger = 35.0;
        public const double HumidityWarning = 65.0;
        public const double HumidityDanger = 80.0;
    }

    public static class Simulation
    {
        public const int UpdateIntervalMs = 1000;
        public const int ChartWindowSize = 60;
        public const int MaxLogEntries = 500;
    }
}
