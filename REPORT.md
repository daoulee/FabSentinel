# Green Vision — 프로젝트 개발 보고서

**AI 기반 반도체 스마트 안전 관리 시스템**  
작성일: 2026-06-30 | 최종 업데이트: 2026-07-13 | 버전: 1.1.0

---

## 1. 프로젝트 개요

### 1.1 목적
반도체 팹(Fab) 환경에서 발생하는 유해 가스(NH₃, CO) 및 초미세먼지(PM2.5, PM10), 온·습도를 실시간으로 수집·분석하여 작업자 안전을 관리하는 **AI 기반 데스크톱 모니터링 시스템**이다.

### 1.2 핵심 기능
- ESP32 기반 다중 센서 데이터 실시간 수집
- 임계값 기반 3단계 알람 (안전 / 경고 / 위험)
- 실시간 라이브 차트 및 히스토리 조회
- AI 비전 검사 (카메라 기반 불량 탐지 — Phase 2 예정)
- 한국어 / 영어 UI 지원
- 클라우드(Supabase) + 시리얼(USB) 이중 데이터 수신 모드

---

## 2. 기술 스택

| 레이어 | 기술 | 버전 |
|--------|------|------|
| **데스크톱 앱** | C# / .NET | 10.0 |
| **UI 프레임워크** | Avalonia UI | 11.2.3 |
| **아키텍처 패턴** | MVVM (CommunityToolkit.Mvvm) | 8.3.2 |
| **차트** | LiveCharts2 (SkiaSharp) | 2.0.0-rc4.5 |
| **아이콘** | Material.Icons.Avalonia | 2.1.0 |
| **DI 컨테이너** | Microsoft.Extensions.DependencyInjection | 8.0.1 |
| **로컬 DB** | SQLite (Microsoft.Data.Sqlite) | 8.0.11 |
| **백엔드 서버** | Python / FastAPI | 0.115.0 |
| **클라우드 DB** | Supabase (PostgreSQL) | — |
| **마이크로컨트롤러** | ESP32 (Arduino) | — |
| **시리얼 통신** | System.IO.Ports | 9.0.0 |
| **IDE** | Antigravity IDE (Google VS Code fork) | 1.107.0 |

---

## 3. 시스템 아키텍처

```
┌─────────────────────────────────────────────────────────────────┐
│                        ESP32 + 센서                              │
│   PMS5003(PM2.5/PM10) · MQ135(NH₃/CO) · DHT22(온도/습도)        │
└───────────────┬─────────────────────────────────────────────────┘
                │
        ┌───────┴──────────────┐
        │                      │
   USB Serial (직접)      WiFi HTTP POST
        │                      │
        │              ┌───────▼─────────────────┐
        │              │  FastAPI 서버 (:8000)    │
        │              │  server/main.py          │
        │              │  - POST /api/sensors     │
        │              │  - GET  /api/sensors/latest │
        │              │  - GET  /api/sensors/history│
        │              └───────┬─────────────────┘
        │                      │ 비동기 저장
        │              ┌───────▼──────────────────┐
        │              │    Supabase (PostgreSQL)  │
        │              │    sensor_readings 테이블 │
        │              └───────┬──────────────────┘
        │                      │
        └──────────┬───────────┘
                   │
     ┌─────────────▼─────────────────────────────────┐
     │         GreenVision 데스크톱 앱                │
     │         (Avalonia UI / .NET 10)                │
     │                                                │
     │  ┌──────────┐  ┌──────────┐  ┌─────────────┐  │
     │  │Dashboard │  │  Fab     │  │   Vision    │  │
     │  │Overview  │  │Monitoring│  │ Inspection  │  │
     │  │Sensors   │  │          │  │  (Phase 2)  │  │
     │  │Alarms    │  │          │  │             │  │
     │  └──────────┘  └──────────┘  └─────────────┘  │
     │  ┌──────────┐  ┌──────────┐  ┌─────────────┐  │
     │  │Hardware  │  │  Logs    │  │  Settings   │  │
     │  │  Status  │  │          │  │  /About     │  │
     │  └──────────┘  └──────────┘  └─────────────┘  │
     └───────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│              Vision Inspection 파이프라인 (Phase 2 진행 중)       │
│                                                                  │
│  [ELP 20MP USB 카메라]                                           │
│         │ USB                                                    │
│  [Raspberry Pi 5] ─── WiFi (핫스팟) ───► GreenVision 앱         │
│   /dev/video0                            http://<pi-ip>:8080    │
│   camera_stream.py                       /stream (MJPEG)        │
│   (MJPEG HTTP 서버)                                              │
└─────────────────────────────────────────────────────────────────┘
```

---

## 4. 프로젝트 파일 구조

```
GreenVision/
├── App.axaml / App.axaml.cs          # 앱 진입점, DI 등록
├── GreenVision.csproj                 # NuGet 패키지 정의
│
├── Core/
│   ├── AppConstants.cs                # 색상, 임계값, 시뮬레이션 상수
│   └── ViewModelBase.cs               # ObservableObject + IDisposable 기반
│
├── Models/
│   ├── SensorReading.cs               # 센서 데이터 모델 (AlarmLevel 자동 계산)
│   ├── AlarmLevel.cs                  # Safe / Warning / Danger enum
│   ├── LogEntry.cs                    # 알람 로그 항목
│   ├── HardwareDevice.cs              # 하드웨어 상태 모델
│   └── InspectionResult.cs            # 비전 검사 결과
│
├── Services/
│   ├── ISensorDataService.cs          # 센서 서비스 인터페이스
│   ├── SensorSimulatorService.cs      # 시뮬레이터 (Gaussian random walk)
│   ├── SerialPortSensorService.cs     # ★ ESP32 USB 시리얼 수신
│   ├── RestApiSensorService.cs        # ★ FastAPI REST 폴링
│   ├── ILocalizationService.cs        # 다국어 인터페이스
│   ├── LocalizationService.cs         # ★ 한국어 / 영어 전환
│   ├── AlarmService.cs                # 알람 처리 및 로그
│   ├── HardwareSimulatorService.cs    # 하드웨어 상태 시뮬레이터
│   ├── LogService.cs                  # 시스템 로그 서비스
│   └── VisionSimulatorService.cs      # 비전 검사 시뮬레이터
│
├── ViewModels/
│   ├── MainWindowViewModel.cs         # 네비게이션, 알람 상태바
│   ├── DashboardViewModel.cs          # ★ 탭 전환 (Overview/Sensors/Alarms)
│   ├── FabMonitoringViewModel.cs      # 센서 배열, 차트, 위험도 계산
│   ├── VisionInspectionViewModel.cs   # 비전 검사 제어
│   ├── HardwareViewModel.cs           # 하드웨어 연결 상태
│   ├── LogsViewModel.cs               # 로그 필터/검색
│   ├── SettingsViewModel.cs           # ★ 언어 전환, 소스 선택
│   ├── SensorCardViewModel.cs         # 개별 센서 카드 VM
│   └── AboutViewModel.cs              # 정보 페이지
│
├── Views/
│   ├── MainWindow.axaml               # 3분할 레이아웃 (헤더/사이드바/콘텐츠)
│   ├── Dashboard/DashboardView.axaml  # ★ 3탭 (Overview/Sensors/Alarms)
│   ├── FabMonitoring/                 # 센서 배열, 환경 차트, 위험도 카드
│   ├── VisionInspection/              # 카메라 미리보기, 검사 이력
│   ├── Hardware/                      # 하드웨어 카드 그리드
│   ├── Logs/                          # 실시간 로그 필터
│   ├── Settings/                      # ★ 언어/소스/임계값 설정
│   └── About/                         # 프로젝트 정보
│
├── Converters/
│   ├── StringToColorBrushConverter.cs # 색상 문자열 → Brush
│   └── StringToMaterialIconKindConverter.cs # ★ 아이콘 이름 → MaterialIconKind
│
├── Styles/
│   └── AppTheme.axaml                 # 전체 다크 테마, 컴포넌트 스타일
│
├── esp32/                             # ★ ESP32 Arduino 펌웨어
│   └── esp32_greenvision.ino
│
└── server/                            # ★ FastAPI 백엔드 서버
    ├── main.py                        # FastAPI 엔드포인트 정의
    ├── camera_stream.py               # ★ ELP 카메라 MJPEG 스트리밍 서버 (Pi에서 실행)
    ├── supabase_schema.sql            # Supabase 테이블/뷰/RLS 스크립트
    ├── requirements.txt               # Python 의존성
    ├── .python-version                # Python 3.14 명시
    └── .env                           # Supabase URL + anon key (로컬 전용)
```

---

## 5. 주요 구현 사항

### 5.1 UI / UX
- **다크 SCADA 스타일** 대시보드 — 산업용 모니터링 UI 레퍼런스 기반 설계
- **3단 컬러 알람 시스템** — Safe(#10B981) / Warning(#F59E0B) / Danger(#EF4444)
- **6개 원형 링 센서 인디케이터** — 실시간 Ellipse Stroke 색상 변경
- **LiveCharts2 그라디언트 라인 차트** — LinearGradientPaint 적용
- **Material Icons** — 사이드바, 센서 카드, 위험도 아이콘 (이모지 제거)
- **탭 전환 Dashboard** — Overview / Sensors(상세 카드 6종) / Alarms
- **한국어/영어 런타임 전환** — LocalizationService + LanguageChanged 이벤트

### 5.2 아키텍처
- **MVVM 패턴** 완전 준수 — View ↔ ViewModel 바인딩, Model 분리
- **ViewLocator** — `GreenVision.ViewModels.XxxViewModel` → `GreenVision.Views.XxxView` 자동 매핑
- **DI (Singleton)** — 모든 서비스/ViewModel App.axaml.cs에서 등록
- **ISensorDataService** 인터페이스 — Simulator / Serial / REST 자유롭게 교체 가능
- **PeriodicTimer** 기반 비동기 루프 — 블로킹 없는 센서 폴링

### 5.3 센서 데이터 파이프라인

| 모드 | 경로 | 구현 파일 |
|------|------|-----------|
| 시뮬레이터 | 앱 내부 Gaussian random walk | `SensorSimulatorService.cs` |
| USB 시리얼 | ESP32 → USB → `SerialPort.ReadLine()` | `SerialPortSensorService.cs` |
| REST API | FastAPI → `GET /api/sensors/latest` 1Hz 폴링 | `RestApiSensorService.cs` |

### 5.4 Supabase 스키마

```sql
sensor_readings (
    id          BIGSERIAL PRIMARY KEY,
    device_id   TEXT,
    timestamp   TIMESTAMPTZ,
    pm25, pm10, nh3, co, temperature, humidity  REAL
)
-- sensor_readings_latest VIEW (device당 최신 1행)
-- RLS: anon read/insert 허용
```

### 5.5 FastAPI 서버 엔드포인트

| Method | Path | 설명 |
|--------|------|------|
| GET | `/health` | 서버 + Supabase 연결 상태 |
| POST | `/api/sensors` | ESP32 데이터 수신 → Supabase 저장 |
| GET | `/api/sensors/latest` | 디바이스별 최신 값 반환 |
| GET | `/api/sensors/history` | 시계열 히스토리 (limit 파라미터) |

### 5.6 알람 임계값

| 센서 | 단위 | WARNING | DANGER |
|------|------|---------|--------|
| PM2.5 | μg/m³ | 35 | 75 |
| PM10 | μg/m³ | 50 | 150 |
| NH₃ | ppm | 25 | 50 |
| CO | ppm | 9 | 35 |
| 온도 | °C | 28 | 35 |
| 습도 | % | 65 | 80 |

---

## 6. 현재 상태 (2026-07-13 기준)

### ✅ 완료
- [x] 전체 UI 레이아웃 (7개 페이지)
- [x] 시뮬레이터 기반 실시간 데이터 갱신
- [x] LiveCharts2 라이브 차트 (그라디언트 영역)
- [x] 6개 원형 센서 링 인디케이터
- [x] 3단계 알람 시스템 + 타워 램프 시뮬레이션
- [x] 한국어/영어 런타임 언어 전환 (LocalizationService)
- [x] Material Icons 적용 (이모지 제거)
- [x] Dashboard 탭 전환 (Overview/Sensors/Alarms)
- [x] Dashboard / FabMonitoring UI 개선 (레이아웃 업데이트)
- [x] FastAPI 서버 구현 및 로컬 실행 확인
- [x] Supabase 프로젝트 생성 + 스키마 적용
- [x] ESP32 Arduino 펌웨어 초안 작성 (`esp32/` 디렉토리로 분리)
- [x] SerialPortSensorService (USB 모드) 구현
- [x] RestApiSensorService (WiFi/REST 모드) 구현
- [x] GitHub 레포지토리 연동 (daoulee/FabSentinel)
- [x] **Raspberry Pi SSH 접속 성공** (teamfive@raspberrypi, 핸드폰 핫스팟 네트워크)
- [x] **ELP 20MP USB 카메라 Pi 인식 확인** (/dev/video0, UVC 드라이버)
- [x] **OpenCV 4.10.0 Pi 설치 확인**
- [x] **MJPEG 스트리밍 서버 구현** (server/camera_stream.py, 포트 8080)
- [x] Pi에 FabSentinel 레포 git clone 완료

### 🔲 미완료 (다음 단계)
- [ ] Pi에서 camera_stream.py 실행 및 스트리밍 동작 확인
- [ ] Mac GreenVision 앱 Vision Inspection 페이지에서 MJPEG 수신 연동
- [ ] Settings UI에서 센서 소스(Simulator/Serial/REST) 런타임 전환
- [ ] App.axaml.cs DI에서 소스 전환 연동
- [ ] ESP32 실제 하드웨어 테스트 및 펌웨어 보정
- [ ] 비전 검사 AI 추론 (ONNX Runtime) — Phase 2 후반
- [ ] 로그 CSV 내보내기 기능
- [ ] 설정값 JSON 파일 저장/로드
- [ ] Windows 배포 테스트

---

## 7. 실행 방법

### 데스크톱 앱
```bash
cd /Users/daul/GreenVision
dotnet run
```

### FastAPI 서버
```bash
cd /Users/daul/GreenVision/server
.venv/bin/python main.py
# → http://localhost:8000
# → Swagger UI: http://localhost:8000/docs
```

### ESP32 펌웨어
1. Arduino IDE에서 `esp32/esp32_greenvision.ino` 열기
2. WiFi SSID / PASSWORD 수정
3. `API_ENDPOINT` 를 FastAPI 서버 IP로 수정
4. ESP32에 업로드

### Raspberry Pi 카메라 스트리밍 서버
```bash
# Pi SSH 접속 (같은 WiFi/핫스팟 필요)
ssh teamfive@raspberrypi.local

# 스트리밍 서버 실행
python3 ~/FabSentinel/server/camera_stream.py
# → http://<pi-ip>:8080/stream 으로 MJPEG 스트리밍
```

**Pi 환경:**
- Raspberry Pi 5 (Linux raspberrypi 6.12.62, aarch64)
- ELP 20MP U3 USB 카메라 → /dev/video0
- OpenCV 4.10.0
- 네트워크: 핸드폰 핫스팟

---

## 8. 개발 환경

| 항목 | 내용 |
|------|------|
| OS | macOS Darwin 25.5.0 (Apple Silicon) |
| IDE | Antigravity IDE v1.107.0 (Google VS Code fork) |
| .NET SDK | 10.0.201 |
| Python | 3.14 |
| 타겟 | net10.0 (macOS + Windows 크로스플랫폼) |
| Supabase 프로젝트 | sjitmxitxmtcfvrutoef |

---

*Green Vision v1.0.0 — 대학교 캡스톤 프로젝트*
