/*
 * GreenVision ESP32 Firmware
 *
 * Reads sensors and either:
 *   (A) Sends JSON over Serial (USB mode)
 *   (B) POSTs JSON to FastAPI server (WiFi mode)
 *
 * Sensors assumed:
 *   - PMS5003  : PM2.5, PM10  (UART)
 *   - MQ135    : NH3, CO      (ADC — calibrate for your unit)
 *   - DHT22    : Temperature, Humidity (GPIO)
 *
 * Board: ESP32 Dev Module | Baud: 115200
 */

#include <Arduino.h>
#include <WiFi.h>
#include <HTTPClient.h>
#include <DHT.h>

// ── Configuration ─────────────────────────────────────────────────────────────
#define USE_WIFI        true          // false = Serial-only mode

const char* WIFI_SSID     = "YOUR_WIFI_SSID";
const char* WIFI_PASSWORD = "YOUR_WIFI_PASSWORD";
const char* API_ENDPOINT  = "http://192.168.1.100:8000/api/sensors";  // FastAPI IP
const char* DEVICE_ID     = "esp32-fab-01";

#define SEND_INTERVAL_MS  1000        // 1 Hz

// ── Sensor pins ───────────────────────────────────────────────────────────────
#define DHT_PIN     4
#define DHT_TYPE    DHT22

#define MQ135_NH3_PIN  34   // ADC1 — NH3 channel
#define MQ135_CO_PIN   35   // ADC1 — CO channel

// PMS5003 connected to UART2 (GPIO16=RX, GPIO17=TX)
#define PMS_RX 16
#define PMS_TX 17

// ── Globals ───────────────────────────────────────────────────────────────────
DHT dht(DHT_PIN, DHT_TYPE);
HardwareSerial pmsSerial(2);

struct PmData { float pm25; float pm10; bool valid; };

// ── Setup ─────────────────────────────────────────────────────────────────────
void setup() {
    Serial.begin(115200);
    pmsSerial.begin(9600, SERIAL_8N1, PMS_RX, PMS_TX);
    dht.begin();
    analogReadResolution(12);

    if (USE_WIFI) {
        Serial.printf("[WiFi] Connecting to %s...\n", WIFI_SSID);
        WiFi.begin(WIFI_SSID, WIFI_PASSWORD);
        uint8_t attempts = 0;
        while (WiFi.status() != WL_CONNECTED && attempts++ < 20) {
            delay(500);
            Serial.print(".");
        }
        if (WiFi.status() == WL_CONNECTED) {
            Serial.printf("\n[WiFi] Connected: %s\n", WiFi.localIP().toString().c_str());
        } else {
            Serial.println("\n[WiFi] Failed — falling back to Serial mode");
        }
    }
}

// ── Main loop ─────────────────────────────────────────────────────────────────
void loop() {
    float temperature = dht.readTemperature();
    float humidity    = dht.readHumidity();

    if (isnan(temperature)) temperature = 0.0f;
    if (isnan(humidity))    humidity    = 0.0f;

    float nh3 = adcToNh3(analogRead(MQ135_NH3_PIN));
    float co  = adcToCo(analogRead(MQ135_CO_PIN));

    PmData pm = readPms5003();
    float pm25 = pm.valid ? pm.pm25 : 0.0f;
    float pm10 = pm.valid ? pm.pm10 : 0.0f;

    // Build JSON
    char json[256];
    snprintf(json, sizeof(json),
        "{\"device_id\":\"%s\","
        "\"pm25\":%.1f,\"pm10\":%.1f,"
        "\"nh3\":%.2f,\"co\":%.2f,"
        "\"temperature\":%.1f,\"humidity\":%.1f}",
        DEVICE_ID, pm25, pm10, nh3, co, temperature, humidity);

    // Always output to Serial (for USB monitoring)
    Serial.println(json);

    // Also POST to FastAPI if WiFi available
    if (USE_WIFI && WiFi.status() == WL_CONNECTED) {
        postToApi(json);
    }

    delay(SEND_INTERVAL_MS);
}

// ── PMS5003 reader ────────────────────────────────────────────────────────────
PmData readPms5003() {
    PmData result = {0, 0, false};
    if (pmsSerial.available() < 32) return result;

    uint8_t buf[32];
    // Find frame start (0x42 0x4D)
    while (pmsSerial.available() >= 2) {
        if (pmsSerial.read() == 0x42 && pmsSerial.peek() == 0x4D) break;
    }
    if (pmsSerial.available() < 30) return result;
    pmsSerial.read(); // consume 0x4D
    pmsSerial.readBytes(buf + 2, 30);

    uint16_t pm25 = (buf[6] << 8) | buf[7];
    uint16_t pm10 = (buf[8] << 8) | buf[9];

    result.pm25  = (float)pm25;
    result.pm10  = (float)pm10;
    result.valid = true;
    return result;
}

// ── MQ135 conversion (approximate — calibrate for your sensor) ───────────────
float adcToNh3(int raw) {
    // Rs/Ro ratio → ppm (MQ135 datasheet curve for NH3)
    float voltage = raw * (3.3f / 4095.0f);
    float rs      = (3.3f - voltage) / voltage * 10.0f;  // load resistor 10kΩ
    float ratio   = rs / 3.6f;                            // Ro calibrated in clean air
    return 102.7f * pow(ratio, -2.26f);                   // datasheet fit
}

float adcToCo(int raw) {
    float voltage = raw * (3.3f / 4095.0f);
    float rs      = (3.3f - voltage) / voltage * 10.0f;
    float ratio   = rs / 3.6f;
    return 605.18f * pow(ratio, -3.937f);
}

// ── HTTP POST ─────────────────────────────────────────────────────────────────
void postToApi(const char* json) {
    HTTPClient http;
    http.begin(API_ENDPOINT);
    http.addHeader("Content-Type", "application/json");
    int code = http.POST(json);
    if (code != 201) {
        Serial.printf("[HTTP] POST failed: %d\n", code);
    }
    http.end();
}
