-- ──────────────────────────────────────────────────────────────────────────
-- GreenVision — Supabase Schema
-- Run this in the Supabase SQL Editor (Dashboard → SQL Editor → New Query)
-- ──────────────────────────────────────────────────────────────────────────

-- 1. Main sensor readings table (time-series)
CREATE TABLE IF NOT EXISTS sensor_readings (
    id          BIGSERIAL PRIMARY KEY,
    device_id   TEXT        NOT NULL DEFAULT 'esp32-fab-01',
    timestamp   TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    pm25        REAL        NOT NULL,
    pm10        REAL        NOT NULL,
    nh3         REAL        NOT NULL,
    co          REAL        NOT NULL,
    temperature REAL        NOT NULL,
    humidity    REAL        NOT NULL
);

-- Index for fast time-range queries
CREATE INDEX IF NOT EXISTS idx_sensor_readings_device_time
    ON sensor_readings (device_id, timestamp DESC);

-- 2. View: latest reading per device (used by desktop app on server restart)
CREATE OR REPLACE VIEW sensor_readings_latest AS
SELECT DISTINCT ON (device_id)
    id, device_id, timestamp,
    pm25, pm10, nh3, co, temperature, humidity
FROM sensor_readings
ORDER BY device_id, timestamp DESC;

-- 3. Enable Row Level Security (allow anonymous reads + inserts for demo)
ALTER TABLE sensor_readings ENABLE ROW LEVEL SECURITY;

CREATE POLICY "Allow anon read"   ON sensor_readings FOR SELECT USING (true);
CREATE POLICY "Allow anon insert" ON sensor_readings FOR INSERT WITH CHECK (true);

-- 4. Auto-delete rows older than 7 days (optional, keeps free tier clean)
-- Uncomment if you want automatic cleanup:
-- CREATE EXTENSION IF NOT EXISTS pg_cron;
-- SELECT cron.schedule('cleanup-old-readings', '0 3 * * *',
--   $$DELETE FROM sensor_readings WHERE timestamp < NOW() - INTERVAL '7 days'$$);
