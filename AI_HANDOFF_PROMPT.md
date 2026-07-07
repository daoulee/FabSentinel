# GreenVision — AI Handoff Prompt

> **For AI assistants (Claude Code, Gemini, ChatGPT, etc.)**
> Read this entire file before doing anything. It gives you complete context to continue development without asking the user to re-explain the project.

---

## What This Project Is

**GreenVision** is an AI-based semiconductor fab safety monitoring desktop application, built as a university capstone project. It monitors 6 environmental sensors in real-time (PM2.5, PM10, NH₃, CO, Temperature, Humidity), displays live charts, triggers 3-level alarms, and supports both simulated and real hardware data.

**Project path:** `/Users/daul/GreenVision/`

---

## Tech Stack — Do Not Change These

| Layer | Technology | Version |
|-------|-----------|---------|
| Desktop App | C# / .NET | **10.0 only** (no .NET 8 — user doesn't have it) |
| UI Framework | Avalonia UI | 11.2.3 |
| MVVM | CommunityToolkit.Mvvm | 8.3.2 |
| Charts | LiveCharts2 SkiaSharp | 2.0.0-rc4.5 |
| Icons | Material.Icons.Avalonia | 2.1.0 |
| DI | Microsoft.Extensions.DependencyInjection | 8.0.1 |
| Database | SQLite (Microsoft.Data.Sqlite) | 8.0.11 |
| Serial | System.IO.Ports | 9.0.0 |
| Backend | Python FastAPI | 0.115.0 |
| Cloud DB | Supabase (PostgreSQL) | — |
| MCU | ESP32 (Arduino) | — |

---

## Critical Architecture Rules

### 1. ViewLocator Pattern
ALL view code-behind files **must** use namespace `GreenVision.Views` (NOT sub-namespaces like `GreenVision.Views.Dashboard`). The ViewLocator maps:
```
GreenVision.ViewModels.DashboardViewModel → GreenVision.Views.DashboardView
```
by replacing `.ViewModels.` with `.Views.` and `ViewModel` with `View`.

### 2. Navigation
Navigation uses `ListBox SelectedItem` two-way binding in `MainWindow.axaml`. Never use command binding from inside DataTemplate — it causes `ArgumentException` at runtime.

```csharp
// MainWindowViewModel.cs
partial void OnSelectedNavItemChanged(NavItemViewModel? value) { ... }
```

### 3. MVVM Pattern
- All ViewModels inherit `ViewModelBase : ObservableObject, IDisposable`
- Use `[ObservableProperty]` for bindable properties (generates `OnPropertyChanged`)
- Use `[RelayCommand]` for commands
- UI thread updates must use `Dispatcher.UIThread.Post()`

### 4. DI Registration
Everything is registered in `App.axaml.cs → ConfigureServices()` as Singleton. Add new services there.

### 5. Sensor Data Flow
`ISensorDataService` is the interface. Three implementations exist:
- `SensorSimulatorService` — Gaussian random walk (current default in DI)
- `SerialPortSensorService` — ESP32 over USB serial (reads JSON lines)
- `RestApiSensorService` — polls `GET /api/sensors/latest` at 1 Hz

**Currently wired in DI:** `SensorSimulatorService` (TODO: make switchable from Settings)

### 6. Icons
Use `Material.Icons.Avalonia` — NOT emojis. Icon names are stored as strings in ViewModels and converted via `StringToMaterialIconKindConverter`. Key nav icons:
```
ViewDashboard, Factory, Camera, Memory, FormatListBulleted, Cog, InformationOutline
```
Sensor icons: `Grain, Blur, Flask, Smoke, Thermometer, WaterPercent`

### 7. Localization
`ILocalizationService` / `LocalizationService` handles Korean/English switching. Keys follow `category.name` format (e.g. `nav.dashboard`, `sensor.pm25`, `status.safe`). Subscribe to `LanguageChanged` event to update UI reactively.

---

## File Structure

```
GreenVision/
├── App.axaml / App.axaml.cs          ← DI registration, LiveCharts init, Material Icons
├── GreenVision.csproj                 ← NuGet refs (net10.0 target)
├── ViewLocator.cs                     ← ViewModel → View auto-mapping
│
├── Core/
│   ├── AppConstants.cs                ← Colors, sensor names, alarm thresholds, sim constants
│   └── ViewModelBase.cs               ← Base class for all ViewModels
│
├── Models/
│   ├── SensorReading.cs               ← Has AlarmLevel (computed) and NormalizedValue (computed)
│   ├── AlarmLevel.cs                  ← enum: Safe, Warning, Danger
│   ├── LogEntry.cs                    ← Alarm log item (Message, SensorName, FormattedTimestamp)
│   ├── HardwareDevice.cs
│   └── InspectionResult.cs
│
├── Services/
│   ├── ISensorDataService.cs          ← Interface: CurrentReadings, ReadingsUpdated, Start/Stop
│   ├── SensorSimulatorService.cs      ← Active: PeriodicTimer + SimulationHelper.WalkValue()
│   ├── SerialPortSensorService.cs     ← Ready: reads {"pm25":x,"pm10":x,...} JSON lines
│   ├── RestApiSensorService.cs        ← Ready: polls FastAPI GET /api/sensors/latest
│   ├── ILocalizationService.cs
│   ├── LocalizationService.cs         ← Korean + English dictionaries, SetLanguage(), LanguageChanged event
│   ├── AlarmService.cs                ← ProcessReadings(), RecentAlarms, TowerLampColor/Status
│   ├── HardwareSimulatorService.cs
│   ├── LogService.cs
│   └── VisionSimulatorService.cs
│
├── ViewModels/
│   ├── MainWindowViewModel.cs         ← NavItems (with LocalizationKey + Icon string), clock timer
│   ├── DashboardViewModel.cs          ← TempHumiditySeries, AirQualitySeries, 6 SensorCards, tab state
│   ├── FabMonitoringViewModel.cs      ← AllSensors[], RiskLevel/Color/Score, EnvironmentSeries, GasSeries
│   ├── SensorCardViewModel.cs         ← Name, Value, DisplayValue, StatusColor, StatusText, NormalizedValue, Icon
│   ├── SettingsViewModel.cs           ← Languages[], SelectedLanguage → calls loc.SetLanguage()
│   └── [others...]
│
├── Views/
│   ├── MainWindow.axaml               ← Grid 52px header / sidebar 160px / content / 32px statusbar
│   ├── Dashboard/DashboardView.axaml  ← 3 tabs: Overview (charts+rings), Sensors (cards), Alarms
│   ├── FabMonitoring/                 ← Risk card (AlertOutline icon), sensor array UniformGrid
│   ├── Settings/SettingsView.axaml    ← Language dropdown at top, thresholds grid
│   └── [others...]
│
├── Converters/
│   ├── StringToColorBrushConverter.cs
│   └── StringToMaterialIconKindConverter.cs  ← Enum.TryParse<MaterialIconKind>
│
├── Styles/AppTheme.axaml              ← Full dark theme; page-title, muted, primary button, nav ListBoxItem
│
├── server/
│   ├── main.py                        ← FastAPI: POST /api/sensors, GET /api/sensors/latest, /history
│   ├── supabase_schema.sql            ← sensor_readings table + latest VIEW + RLS policies
│   ├── esp32_greenvision.ino          ← Arduino sketch: DHT22 + PMS5003 + MQ135 → Serial + HTTP POST
│   ├── requirements.txt
│   └── .env                           ← SUPABASE_URL + SUPABASE_ANON_KEY (do not commit)
│
├── REPORT.md                          ← Korean project report
└── AI_HANDOFF_PROMPT.md               ← This file
```

---

## Data Flow Details

### Sensor Pipeline
```
SensorSimulatorService (or Serial/REST)
    │
    └── fires ReadingsUpdated event (IReadOnlyList<SensorReading>)
            │
            ├── DashboardViewModel.OnReadingsUpdated()
            │       → updates 6 SensorCardViewModels
            │       → pushes to ObservableCollection<double> for charts (Math.Round to 1dp)
            │       → calls AlarmService.ProcessReadings()
            │
            └── FabMonitoringViewModel.OnReadingsUpdated()
                    → updates AllSensors[]
                    → pushes to history collections
```

### Chart Data
- `TempHumiditySeries`: Temperature (teal #00D2B5) + Humidity (purple #8855FF) with LinearGradientPaint fills
- `AirQualitySeries`: PM2.5 (red #EF4444) + CO (orange #F59E0B) with gradient fills
- Values rounded to 1 decimal before storage to keep tooltip clean (rc4.5 lacks easy TooltipLabelFormatter)
- X-axis hidden; Y-axis shows values with muted color paint

### Alarm Logic
`AlarmService.ProcessReadings()` → sets `TowerLampColor`, `TowerLampStatus`, appends to `RecentAlarms` (max 8 in dashboard)

---

## Backend / Cloud

### FastAPI Server (server/main.py)
- Run: `cd server && .venv/bin/python main.py`
- Base URL: `http://localhost:8000`
- Swagger docs: `http://localhost:8000/docs`

**Key endpoints:**
```
POST /api/sensors          ← ESP32 sends here (201 on success)
GET  /api/sensors/latest   ← Desktop app polls here (1 Hz)
GET  /api/sensors/history  ← Historical data from Supabase
GET  /health               ← {"status":"ok","supabase":true}
```

**ESP32 JSON payload format:**
```json
{
  "device_id": "esp32-fab-01",
  "pm25": 18.3, "pm10": 24.1,
  "nh3": 4.2,   "co": 2.8,
  "temperature": 22.5, "humidity": 54.1
}
```

**FastAPI response format (GET /latest):**
```json
[{
  "device_id": "esp32-fab-01",
  "timestamp": "2026-06-30T11:31:54Z",
  "readings": [
    {"name": "PM2.5", "value": 18.3, "unit": "μg/m³"},
    ...
  ]
}]
```

### Supabase
- Project ref: `sjitmxitxmtcfvrutoef`
- URL: `https://sjitmxitxmtcfvrutoef.supabase.co`
- Table: `sensor_readings` (id, device_id, timestamp, pm25, pm10, nh3, co, temperature, humidity)
- View: `sensor_readings_latest` (DISTINCT ON device_id, ORDER BY timestamp DESC)
- RLS: anon read + insert allowed

---

## What's Done ✅

- [x] Full dark SCADA-style UI with 7 pages
- [x] Real-time simulator with PeriodicTimer + Gaussian random walk
- [x] LiveCharts2 gradient area line charts (live updating)
- [x] 6 circular ring sensor indicators (Ellipse + status color)
- [x] 3-level alarm system + tower lamp simulation
- [x] Korean/English runtime language switching (LocalizationService)
- [x] Material Icons throughout (no emojis)
- [x] Dashboard 3-tab switching (Overview / Sensors / Alarms)
- [x] Chart tooltip values rounded to 1–2 decimal places
- [x] FastAPI server running, Supabase connected and tested
- [x] SerialPortSensorService — reads ESP32 JSON over USB
- [x] RestApiSensorService — polls FastAPI at 1 Hz
- [x] ESP32 Arduino firmware draft (WiFi + Serial dual mode)
- [x] Supabase schema deployed (table + view + RLS)

---

## What's NOT Done Yet 🔲 — Pick Up Here

### Priority 1: Sensor Source Switching (Settings UI)
The 3 sensor services exist but DI always wires `SensorSimulatorService`. Need:
1. Add `SensorSource` enum (Simulator / Serial / Rest) to `AppSettings.cs`
2. Add `SelectedSensorSource` + `AvailableSources[]` to `SettingsViewModel.cs`
3. Add source selector ComboBox to `SettingsView.axaml`
4. On change, stop old service, start new one (requires a factory or service locator pattern)
5. `SerialPortSensorService.UpdatePort(portName)` and `RestApiSensorService.UpdateBaseUrl(url)` already exist

### Priority 2: Persist Settings to Disk
`SettingsViewModel.SaveSettingsAsync()` is a stub. Need:
- Serialize settings to `~/.greenvision/settings.json` (or use SQLite)
- Load on startup in `App.axaml.cs`
- Apply: set language, sensor source, thresholds, API URL, COM port

### Priority 3: Log CSV Export
`LogsView.axaml` has an "Export CSV" button. `LogsViewModel` needs `ExportCsvCommand` implementation using `CsvHelper` or manual `StringBuilder`.

### Priority 4: Vision Inspection (Phase 2)
`VisionInspectionView.axaml` and `VisionInspectionViewModel.cs` are stubs. Full implementation needs:
- `OpenCvSharp4` for camera capture
- `Microsoft.ML.OnnxRuntime` for defect detection model
- Bounding box overlay on camera feed

### Priority 5: Windows Deployment Test
Build with `dotnet publish -r win-x64 --self-contained` and verify on Windows with .NET 10 SDK.

---

## Common Gotchas

1. **Target framework is `net10.0`** — never change to net8.0 or net6.0
2. **All View code-behind namespaces must be `GreenVision.Views`** — not sub-namespaces
3. **LiveCharts rc4.5 does not expose `TooltipLabelFormatter` on `LineSeries<double>` via object initializer** — round values before storing instead
4. **`Classes.tab-active` binding with hyphens** — renamed to `Classes.tabactive` in DashboardView.axaml to avoid XAML parse issues
5. **Material.Icons.Avalonia 2.x requires `<mi:MaterialIconStyles />` in App.axaml**, NOT `StyleInclude` (that's for 1.x)
6. **`Dispatcher.UIThread.Post()`** is required for all UI updates coming from background sensor threads
7. **Serial port auto-retries** after 3s on disconnect — this is intentional behavior in `SerialPortSensorService`
8. **Supabase anon key is in `server/.env`** — do not hardcode it in source files, do not commit `.env`

---

## How to Run

```bash
# Desktop app (macOS)
cd /Users/daul/GreenVision
dotnet run

# FastAPI server
cd /Users/daul/GreenVision/server
.venv/bin/python main.py

# Both together
dotnet run &
cd server && .venv/bin/python main.py
```

App opens at 1400×860, dark theme, Korean by default. Change language in Settings page.

---

## User Context

- **User:** Korean university student, capstone project, comfortable with C#/.NET
- **IDE:** Antigravity IDE (Google's VS Code fork) — same extensions as VS Code
- **Machine:** macOS Apple Silicon (Darwin 25.5.0)
- **Windows target:** Final product must also run on Windows (Avalonia is cross-platform)
- **Hardware:** ESP32 with PMS5003 (PM), MQ135 (gas), DHT22 (temp/humidity) sensors
- **Communication style:** Mix of Korean and English, prefers concise direct answers
