namespace GreenVision.Services;

public sealed class LocalizationService : ILocalizationService
{
    private string _lang = "ko";
    public string CurrentLanguage => _lang;
    public event EventHandler? LanguageChanged;

    private static readonly Dictionary<string, Dictionary<string, string>> _dict = new()
    {
        ["ko"] = new()
        {
            // ── Navigation ──────────────────────────────────
            ["nav.dashboard"]  = "대시보드",
            ["nav.fab"]        = "팹 모니터링",
            ["nav.vision"]     = "비전 검사",
            ["nav.hardware"]   = "하드웨어",
            ["nav.logs"]       = "로그",
            ["nav.settings"]   = "설정",
            ["nav.about"]      = "정보",

            // ── Sensors ──────────────────────────────────────
            ["sensor.pm25"]        = "초미세먼지",
            ["sensor.pm10"]        = "미세먼지",
            ["sensor.nh3"]         = "암모니아",
            ["sensor.co"]          = "일산화탄소",
            ["sensor.temperature"] = "온도",
            ["sensor.humidity"]    = "습도",

            // ── Alarm Status ──────────────────────────────────
            ["status.safe"]    = "안전",
            ["status.warning"] = "경고",
            ["status.danger"]  = "위험",

            // ── Device Status ─────────────────────────────────
            ["device.online"]      = "온라인",
            ["device.offline"]     = "오프라인",
            ["device.connecting"]  = "연결 중",
            ["device.error"]       = "오류",

            // ── Inspection ────────────────────────────────────
            ["inspection.pass"]    = "합격",
            ["inspection.fail"]    = "불합격",
            ["inspection.pending"] = "대기 중",
            ["inspection.title"]   = "비전 검사",
            ["inspection.connect"] = "카메라 연결",
            ["inspection.run"]     = "검사 실행",

            // ── Dashboard ─────────────────────────────────────
            ["dashboard.title"]        = "실시간 모니터링",
            ["dashboard.env_chart"]    = "환경 모니터링",
            ["dashboard.air_chart"]    = "대기질",
            ["dashboard.tower"]        = "타워 램프",
            ["dashboard.recent_alarm"] = "최근 알람",

            // ── Pages ─────────────────────────────────────────
            ["page.fab"]      = "팹 환경 모니터링",
            ["page.hardware"] = "하드웨어 상태",
            ["page.logs"]     = "시스템 로그",
            ["page.settings"] = "설정",
            ["page.about"]    = "정보",

            // ── Settings ──────────────────────────────────────
            ["settings.language"]       = "언어",
            ["settings.connection"]     = "연결 설정",
            ["settings.sensor_source"]  = "센서 데이터 소스",
            ["settings.serial_port"]    = "시리얼 포트 (ESP32)",
            ["settings.api_url"]        = "REST API 주소",
            ["settings.camera"]         = "카메라 인덱스",
            ["settings.interval"]       = "갱신 주기 (ms)",
            ["settings.thresholds"]     = "알람 임계값",
            ["settings.save"]           = "설정 저장",
            ["settings.reset"]          = "기본값 초기화",
            ["settings.save_ok"]        = "설정이 저장되었습니다.",

            // ── Source types ─────────────────────────────────
            ["source.simulator"]   = "시뮬레이터 (데모)",
            ["source.serial"]      = "시리얼 포트 (ESP32)",
            ["source.rest"]        = "REST API (FastAPI)",

            // ── Logs ──────────────────────────────────────────
            ["log.search"]   = "메시지 검색...",
            ["log.export"]   = "CSV 내보내기",
            ["log.clear"]    = "필터 초기화",
            ["log.total"]    = "전체",
            ["log.errors"]   = "오류",
            ["log.warnings"] = "경고",
        },

        ["en"] = new()
        {
            // ── Navigation ──────────────────────────────────
            ["nav.dashboard"]  = "Dashboard",
            ["nav.fab"]        = "Fab Monitoring",
            ["nav.vision"]     = "Vision Inspection",
            ["nav.hardware"]   = "Hardware",
            ["nav.logs"]       = "Logs",
            ["nav.settings"]   = "Settings",
            ["nav.about"]      = "About",

            // ── Sensors ──────────────────────────────────────
            ["sensor.pm25"]        = "PM2.5",
            ["sensor.pm10"]        = "PM10",
            ["sensor.nh3"]         = "NH3",
            ["sensor.co"]          = "CO",
            ["sensor.temperature"] = "Temperature",
            ["sensor.humidity"]    = "Humidity",

            // ── Alarm Status ──────────────────────────────────
            ["status.safe"]    = "SAFE",
            ["status.warning"] = "WARNING",
            ["status.danger"]  = "DANGER",

            // ── Device Status ─────────────────────────────────
            ["device.online"]      = "ONLINE",
            ["device.offline"]     = "OFFLINE",
            ["device.connecting"]  = "CONNECTING",
            ["device.error"]       = "ERROR",

            // ── Inspection ────────────────────────────────────
            ["inspection.pass"]    = "PASS",
            ["inspection.fail"]    = "FAIL",
            ["inspection.pending"] = "PENDING",
            ["inspection.title"]   = "Vision Inspection",
            ["inspection.connect"] = "Connect Camera",
            ["inspection.run"]     = "Run Inspection",

            // ── Dashboard ─────────────────────────────────────
            ["dashboard.title"]        = "Live Monitoring",
            ["dashboard.env_chart"]    = "Environmental Monitoring",
            ["dashboard.air_chart"]    = "Air Quality",
            ["dashboard.tower"]        = "Tower Lamp",
            ["dashboard.recent_alarm"] = "Recent Alarms",

            // ── Pages ─────────────────────────────────────────
            ["page.fab"]      = "Fab Environment Monitoring",
            ["page.hardware"] = "Hardware Status",
            ["page.logs"]     = "System Logs",
            ["page.settings"] = "Settings",
            ["page.about"]    = "About",

            // ── Settings ──────────────────────────────────────
            ["settings.language"]       = "Language",
            ["settings.connection"]     = "Connection",
            ["settings.sensor_source"]  = "Sensor Data Source",
            ["settings.serial_port"]    = "Serial Port (ESP32)",
            ["settings.api_url"]        = "REST API Base URL",
            ["settings.camera"]         = "Camera Index",
            ["settings.interval"]       = "Update Interval (ms)",
            ["settings.thresholds"]     = "Alarm Thresholds",
            ["settings.save"]           = "Save Settings",
            ["settings.reset"]          = "Reset Defaults",
            ["settings.save_ok"]        = "Settings saved successfully.",

            // ── Source types ─────────────────────────────────
            ["source.simulator"]   = "Simulator (Demo)",
            ["source.serial"]      = "Serial Port (ESP32)",
            ["source.rest"]        = "REST API (FastAPI)",

            // ── Logs ──────────────────────────────────────────
            ["log.search"]   = "Search messages...",
            ["log.export"]   = "Export CSV",
            ["log.clear"]    = "Clear Filter",
            ["log.total"]    = "Total",
            ["log.errors"]   = "Errors",
            ["log.warnings"] = "Warnings",
        }
    };

    public string Get(string key)
    {
        if (_dict.TryGetValue(_lang, out var d) && d.TryGetValue(key, out var v)) return v;
        if (_dict.TryGetValue("en", out var en) && en.TryGetValue(key, out var ev)) return ev;
        return $"[{key}]";
    }

    public void SetLanguage(string language)
    {
        var code = language is "한국어" or "ko" ? "ko" : "en";
        if (code == _lang) return;
        _lang = code;
        LanguageChanged?.Invoke(this, EventArgs.Empty);
    }
}
