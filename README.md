 # 🏭 FabSentinel (Green Vision)                                                                                                                     
                                                                                                                                                        
    > **반도체 가상 팹(FAB) 환경 모니터링 및 실시간 오염도·안전 감지 시스템**                                                                           
    > *Virtual Semiconductor FAB Cleanroom Pollution Monitoring & Safety Intelligence System*                                                           
                                                                                                                                                        
    [![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)                                 
    [![Avalonia UI](https://img.shields.io/badge/UI-Avalonia_11.2-8E44AD?logo=avalonia)](https://avaloniaui.net/)                                       
    [![FastAPI](https://img.shields.io/badge/Backend-FastAPI-009688?logo=fastapi&logoColor=white)](https://fastapi.tiangolo.com/)                       
    [![Supabase](https://img.shields.io/badge/Database-Supabase-3ECF8E?logo=supabase&logoColor=white)](https://supabase.com/)                           
    [![ESP32](https://img.shields.io/badge/IoT-ESP32-E7352C?logo=espressif&logoColor=white)](https://www.espressif.com/)                                
    [![Raspberry Pi](https://img.shields.io/badge/Edge-Raspberry_Pi_5-C51A4A?logo=raspberrypi&logoColor=white)](https://www.raspberrypi.com/)           
    [![OpenCV](https://img.shields.io/badge/Vision-OpenCV_4.10-5C3EE8?logo=opencv&logoColor=white)](https://opencv.org/)                                
                                                                                                                                                        
    ---                                                                                                                                                 
                                                                                                                                                        
    ## 📌 1. 프로젝트 개요 (Overview)                                                                                                                   
                                                                                                                                                        
    반도체 팹(FAB) 클린룸 환경은 극미세 입자 및 유해 화학 물질 관리가 제품 수율과 작업자 안전에 직결되는 고위험 구역입니다.                             
                                                                                                                                                        
    **FabSentinel**은 **반도체 가상 팹실 환경 및 가상 센서 데이터셋을 구축**하여, 팹실 내부의 **유해 가스(NH₃, CO), 초미세먼지(PM2.5, PM10), 온·습도 등 
  복합 오염도를 실시간 감지·분석**하는 **산업용 SCADA 스타일 AI 안전 모니터링 시스템**입니다.                                                           
                                                                                                                                                        
    * **개발 형태**: 대학교 캡스톤 프로젝트 (Green Vision)                                                                                              
    * **주요 역할**: 가상 팹 환경 데이터 파이프라인 설계, C# Avalonia 기반 크로스플랫폼 모니터링 데스크톱 앱 개발, 백엔드 API 및 IoT 통신 연동          
                                                                                                                                                        
    ---                                                                                                                                                 
                                                                                                                                                        
    ## 🎯 2. 핵심 기능 (Key Features)                                                                                                                   
                                                                                                                                                        
    ### 1) 🧪 가상 팹실(FAB) 환경 데이터 시뮬레이션                                                                                                     
    * 가우시안 랜덤 워크(Gaussian Random Walk) 및 실제 반도체 공정 시나리오 기반의 가상 센서 데이터셋 생성                                              
    * 하드웨어 없이도 유해 가스 누출, 클린룸 파티클 급증 등의 이상 상황 시뮬레이션 및 검증 가능                                                         
                                                                                                                                                        
    ### 2) 📊 실시간 오염도 감지 & 3단계 스마트 알람 (SCADA Dashboard)                                                                                  
    * **3단계 상태 판별**: 정밀 임계값 기반 `SAFE(안전)` / `WARNING(경고)` / `DANGER(위험)` 단계별 타워 램프 및 인디케이터 시각화                       
    * **실시간 라이브 차트**: LiveCharts2 기반의 실시간 시계열 환경 변화 추이 및 위험도 트렌드 분석                                                     
    * **6종 복합 환경 지표 감시**: PM2.5, PM10, NH₃, CO, 온도, 습도                                                                                     
                                                                                                                                                        
    ### 3) 🔄 유연한 다중 데이터 파이프라인 (Pluggable Architecture)                                                                                    
    * `ISensorDataService` 인터페이스 기반의 느슨한 결합 설계로 데이터 소스 자유 전환:                                                                  
      1. **가상 시뮬레이터 모드**: 앱 내부 가상 팹 데이터셋 생성                                                                                        
      2. **USB Serial 모드**: ESP32 보드와 USB 시리얼 직접 통신                                                                                         
      3. **REST API / Cloud 모드**: FastAPI + Supabase 실시간 폴링/스트리밍                                                                             
                                                                                                                                                        
    ### 4) 📷 비전 검사 & 에지 스트리밍 (Vision Inspection)                                                                                             
    * Raspberry Pi 5 + ELP 20MP 산업용 카메라 기반 MJPEG 실시간 영상 스트리밍 (OpenCV 4.10)                                                             
    * 팹실 내부 원격 육안 검사 및 비전 AI 불량 검사 기반 마련                                                                                           
                                                                                                                                                        
    ---                                                                                                                                                 
                                                                                                                                                        
    ## 🏗️ 3. 시스템 아키텍처 (System Architecture)                                                                                                      
                                                                                                                                                        
                                                                                                                                                        
  [ 가상 팹 시뮬레이터 / ESP32 센서 ]                                                                                                                   
  │                                                                                                                                                     
  ▼ (WiFi / HTTP POST or USB Serial)                                                                                                                    
  [ FastAPI Backend Server ]                                                                                                                            
  │                                                                                                                                                     
  ├──────────► [ Supabase DB (PostgreSQL) ]                                                                                                             
  ▼                                                                                                                                                     
  [ FabSentinel Desktop App (C# Avalonia MVVM) ]                                                                                                        
  ├── SCADA 대시보드 (오염도 실시간 감지 & 차트)                                                                                                        
  ├── 3단계 위험도 알람 & 타워 램프 제어                                                                                                                
  └── MJPEG 카메라 스트리밍 뷰어 (Raspberry Pi 5 + OpenCV)                                                                                              
                                                                                                                                                        
                                                                                                                                                        
    ---                                                                                                                                                 
                                                                                                                                                        
    ## ⚠️ 4. 오염도 감지 임계값 기준                                                                                                                    
                                                                                                                                                        
    | 감시 항목 | 단위 | WARNING (경고) | DANGER (위험) | 비고 |                                                                                        
    |:---|:---:|:---:|:---:|---|                                                                                                                        
    | **초미세먼지 (PM2.5)** | $\mu g/m^3$ | 35 | 75 | 클린룸 파티클 청정도 감시 |                                                                      
    | **미세먼지 (PM10)** | $\mu g/m^3$ | 50 | 150 | 외기 유입 및 필터 이상 감지 |                                                                      
    | **암모니아 ($NH_3$)** | ppm | 25 | 50 | 식각/세정 공정 유해 가스 감시 |                                                                           
    | **일산화탄소 ($CO$)** | ppm | 9 | 35 | 설비 불완전 연소 및 화재 징후 |                                                                            
    | **온도** | °C | 28 | 35 | 공정 정밀 온·습도 유지 |                                                                                                
    | **습도** | % | 65 | 80 | 정전기 방지 및 결로 방지 |                                                                                               
                                                                                                                                                        
    ---                                                                                                                                                 
                                                                                                                                                        
    ## 🛠️ 5. 기술 스택 (Tech Stack)                                                                                                                     
                                                                                                                                                        
    | 구분 | 기술 스택 | 설명 |                                                                                                                         
    |---|---|---|                                                                                                                                       
    | **Desktop App** | C# .NET 10, Avalonia UI 11.2 | 크로스플랫폼 (macOS / Windows) 데스크톱 SCADA 클라이언트 |                                       
    | **Architecture** | MVVM Pattern, CommunityToolkit.Mvvm | 반응형 UI 데이터 바인딩 및 유지보수성 극대화 |                                           
    | **Visualization**| LiveCharts2, Material.Icons.Avalonia | 실시간 그라디언트 차트 및 산업용 UI 컴포넌트 |                                          
    | **Backend** | Python 3.14, FastAPI, Uvicorn | 센서 데이터 수집 및 RESTful API 서버 |                                                              
    | **Database** | Supabase (PostgreSQL) | 시계열 센서 로그 저장, View 테이블, RLS 보안 |                                                             
    | **Edge & IoT** | Raspberry Pi 5, ESP32, C/C++ (Arduino) | 온습도/가스 센서 데이터 송신 및 엣지 스트리밍 |                                         
    | **Vision** | OpenCV 4.10, ELP 20MP USB Camera | MJPEG 실시간 영상 스트리밍 서버 |                                                                 
                                                                                                                                                        
    ---                                                                                                                                                 
                                                                                                                                                        
    ## 📁 6. 프로젝트 구조 (Structure)                                                                                                                  
                                                                                                                                                        
                                                                                                                                                        
  FabSentinel/                                                                                                                                          
  ├── Core/                      # 인터페이스 및 데이터 모델 정의                                                                                       
  ├── Services/                  # 데이터 수집 (Simulator / Serial / REST), 다국어, 로그 서비스                                                         
  ├── ViewModels/                # MVVM 뷰모델 (Dashboard, Monitoring, Vision, Hardware 등)                                                             
  ├── Views/                     # Avalonia XAML 화면 (SCADA 대시보드, 탭 뷰, 실시간 차트)                                                              
  ├── Styles/                    # 산업용 다크 SCADA 테마 정의                                                                                          
  ├── esp32/                     # ESP32 센서 수집 Arduino 펌웨어                                                                                       
  │   └── esp32_greenvision.ino                                                                                                                         
  └── server/                    # FastAPI 백엔드 & OpenCV 카메라 스트리밍 서버                                                                         
  ├── main.py                # REST API 엔드포인트                                                                                                      
  ├── camera_stream.py       # Pi 카메라 MJPEG 스트리밍 서버                                                                                            
  └── supabase_schema.sql    # 데이터베이스 스키마                                                                                                      
                                                                                                                                                        
                                                                                                                                                        
    ---                                                                                                                                                 
                                                                                                                                                        
    ## 🚀 7. 실행 방법 (Getting Started)                                                                                                                
                                                                                                                                                        
    ### 1) 데스크톱 모니터링 앱 실행                                                                                                                    
    ```bash                                                                                                                                             
    # .NET 10 환경                                                                                                                                      
    dotnet restore                                                                                                                                      
    dotnet run                                                                                                                                          
                                                                                                                                                        
  ### 2) FastAPI 백엔드 서버 실행                                                                                                                       
                                                                                                                                                        
    cd server                                                                                                                                           
    python -m venv .venv                                                                                                                                
    source .venv/bin/activate  # Windows: .venv\Scripts\activate                                                                                        
    pip install -r requirements.txt                                                                                                                     
    python main.py                                                                                                                                      
    # → Swagger UI: http://localhost:8000/docs                                                                                                          
                                                                                                                                                        
  ### 3) Raspberry Pi 카메라 스트리밍 실행                                                                                                              
                                                                                                                                                        
    python3 server/camera_stream.py                                                                                                                     
    # → http://<RaspberryPi_IP>:8080/stream                                                                                                             
                                                                                                                                                        
                                                                                                                                                        
    ---                                                                                                                             
