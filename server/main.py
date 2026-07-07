"""
GreenVision FastAPI Server
- Receives sensor data from ESP32 (POST /api/sensors)
- Stores readings in Supabase
- Serves latest readings to the desktop app (GET /api/sensors/latest)
"""

from __future__ import annotations

import os
import asyncio
from datetime import datetime, timezone, timedelta

KST = timezone(timedelta(hours=9))
from typing import Optional

from fastapi import FastAPI, HTTPException, BackgroundTasks
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel, Field
from dotenv import load_dotenv

load_dotenv()

# ── Supabase client ───────────────────────────────────────────────────────────
SUPABASE_URL = os.getenv("SUPABASE_URL", "")
SUPABASE_KEY = os.getenv("SUPABASE_ANON_KEY", "")

supabase_client = None
if SUPABASE_URL and SUPABASE_KEY:
    try:
        from supabase import create_client
        supabase_client = create_client(SUPABASE_URL, SUPABASE_KEY)
        print(f"[Supabase] Connected to {SUPABASE_URL}")
    except Exception as e:
        print(f"[Supabase] Connection failed: {e}")
else:
    print("[Supabase] URL/KEY not set — running without cloud storage")

# ── In-memory cache (latest readings) ─────────────────────────────────────────
_latest: dict[str, dict] = {}

# ── FastAPI app ───────────────────────────────────────────────────────────────
app = FastAPI(
    title="GreenVision Sensor API",
    description="Bridge between ESP32 sensors and the GreenVision desktop app",
    version="1.0.0",
)

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_methods=["*"],
    allow_headers=["*"],
)

# ── Schemas ───────────────────────────────────────────────────────────────────
class SensorPayload(BaseModel):
    """JSON body sent by the ESP32."""
    device_id: str = Field(default="esp32-fab-01", description="Unique device identifier")
    pm25:        float = Field(..., ge=0, description="PM2.5 μg/m³")
    pm10:        float = Field(..., ge=0, description="PM10 μg/m³")
    nh3:         float = Field(..., ge=0, description="NH3 ppm")
    co:          float = Field(..., ge=0, description="CO ppm")
    temperature: float = Field(..., description="Temperature °C")
    humidity:    float = Field(..., ge=0, le=100, description="Humidity %")
    timestamp:   Optional[str] = Field(None, description="ISO8601 — auto-set if omitted")


class SensorReading(BaseModel):
    name:  str
    value: float
    unit:  str


class LatestResponse(BaseModel):
    device_id:  str
    timestamp:  str
    readings:   list[SensorReading]


# ── Routes ────────────────────────────────────────────────────────────────────
@app.get("/", tags=["Health"])
async def root():
    return {"status": "ok", "service": "GreenVision Sensor API"}


@app.get("/health", tags=["Health"])
async def health():
    return {
        "status": "ok",
        "supabase": supabase_client is not None,
        "cached_devices": list(_latest.keys()),
    }


@app.post("/api/sensors", tags=["Sensors"], status_code=201)
async def receive_sensor_data(payload: SensorPayload, background: BackgroundTasks):
    """
    ESP32 calls this endpoint every second with sensor readings.
    Data is cached in-memory for instant desktop-app polling,
    and written to Supabase in the background.
    """
    ts = payload.timestamp or datetime.now(KST).isoformat()

    _latest[payload.device_id] = {
        "device_id":  payload.device_id,
        "timestamp":  ts,
        "pm25":        payload.pm25,
        "pm10":        payload.pm10,
        "nh3":         payload.nh3,
        "co":          payload.co,
        "temperature": payload.temperature,
        "humidity":    payload.humidity,
    }

    if supabase_client:
        background.add_task(_store_supabase, _latest[payload.device_id])

    return {"accepted": True, "timestamp": ts}


@app.get("/api/sensors/latest", tags=["Sensors"], response_model=list[LatestResponse])
async def get_latest_readings(device_id: Optional[str] = None):
    """
    Desktop app polls this to get the latest reading from each device.
    Optionally filter by device_id.
    """
    if not _latest:
        # Fall back to Supabase if cache is empty (e.g. server just restarted)
        if supabase_client:
            return await _fetch_supabase_latest(device_id)
        raise HTTPException(status_code=503, detail="No sensor data available yet")

    results = []
    for did, row in _latest.items():
        if device_id and did != device_id:
            continue
        results.append(_row_to_response(row))

    return results


@app.get("/api/sensors/history", tags=["Sensors"])
async def get_history(
    device_id: str = "esp32-fab-01",
    limit: int = 60,
    sensor: Optional[str] = None,
):
    """Return historical readings from Supabase (for chart replay)."""
    if not supabase_client:
        raise HTTPException(status_code=503, detail="Supabase not configured")

    query = (
        supabase_client.table("sensor_readings")
        .select("*")
        .eq("device_id", device_id)
        .order("timestamp", desc=True)
        .limit(limit)
    )
    result = query.execute()
    rows = list(reversed(result.data or []))

    if sensor:
        return [{"timestamp": r["timestamp"], "value": r.get(sensor)} for r in rows]
    return rows


# ── Helpers ───────────────────────────────────────────────────────────────────
def _row_to_response(row: dict) -> LatestResponse:
    return LatestResponse(
        device_id=row["device_id"],
        timestamp=row["timestamp"],
        readings=[
            SensorReading(name="PM2.5",       value=row["pm25"],        unit="μg/m³"),
            SensorReading(name="PM10",         value=row["pm10"],        unit="μg/m³"),
            SensorReading(name="NH3",          value=row["nh3"],         unit="ppm"),
            SensorReading(name="CO",           value=row["co"],          unit="ppm"),
            SensorReading(name="Temperature",  value=row["temperature"], unit="°C"),
            SensorReading(name="Humidity",     value=row["humidity"],    unit="%"),
        ],
    )


async def _store_supabase(row: dict):
    try:
        supabase_client.table("sensor_readings").insert(row).execute()
    except Exception as e:
        print(f"[Supabase] Insert failed: {e}")


async def _fetch_supabase_latest(device_id: Optional[str]) -> list[LatestResponse]:
    try:
        query = (
            supabase_client.table("sensor_readings_latest")
            .select("*")
        )
        if device_id:
            query = query.eq("device_id", device_id)
        result = query.execute()
        return [_row_to_response(r) for r in (result.data or [])]
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


# ── Entry point ───────────────────────────────────────────────────────────────
if __name__ == "__main__":
    import uvicorn
    uvicorn.run("main:app", host="0.0.0.0", port=8000, reload=True)
