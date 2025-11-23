# MPU-6050 Resilience Testing Guide

## Quick Test Commands

### 1. Check Build Status
```bash
cd /home/andrew/Desktop/cstest/nocluee/ConsoleApp1
dotnet build
# Expected: Build succeeded. 0 Warning(s), 0 Error(s)
```

### 2. Start Application
```bash
dotnet run
# Should start successfully even if MPU-6050 is not connected
```

### 3. Test Sensor Status
```bash
# Check if sensor is available
curl http://localhost:5000/gyro/status

# Expected responses:
# If sensor is connected:
# {"available":true,"lastError":null,"lastUpdate":"2025-11-23T..."}

# If sensor is NOT connected:
# {"available":false,"lastError":"Error 5 performing I2C data transfer","lastUpdate":"..."}
```

### 4. Test Gyro Endpoints (Sensor Connected)
```bash
# Get raw sensor data
curl http://localhost:5000/gyro/raw
# Expected: {"ax":0.01,"ay":0.02,"az":1.0,"gx":0.5,"gy":-0.3,"gz":0.1}

# Get angles
curl http://localhost:5000/gyro/angles
# Expected: {"pitch":1.5,"roll":-2.3,"yaw":0.0}

# Get magnitude
curl http://localhost:5000/gyro/magnitude
# Expected: {"magnitude":12.5}
```

### 5. Test Gyro Endpoints (Sensor Disconnected)
```bash
# All gyro endpoints should return HTTP 503
curl -i http://localhost:5000/gyro/raw

# Expected response:
# HTTP/1.1 503 Service Unavailable
# Content-Type: application/problem+json
# 
# {
#   "type": "https://tools.ietf.org/html/rfc9110#section-15.6.4",
#   "title": "MPU-6050 sensor unavailable",
#   "status": 503,
#   "detail": "MPU-6050 sensor is not available. Last error: ..."
# }
```

### 6. Verify Other Endpoints Still Work
```bash
# LED should work regardless of sensor status
curl http://localhost:5000/gpio/on
# Expected: {"ok":true,"led":"on"}

curl http://localhost:5000/gpio/off
# Expected: {"ok":true,"led":"off"}

# Servo should work regardless of sensor status
curl http://localhost:5000/rotate/90
# Expected: {"ok":true,"angle":90}

# Health endpoint
curl http://localhost:5000/health
# Expected: {"status":"ok",...}
```

## Test Scenarios

### Scenario 1: I2C Disabled

**Setup:**
```bash
# Disable I2C
sudo raspi-config
# Navigate to: Interface Options → I2C → No
# Reboot
sudo reboot
```

**Expected Behavior:**
- Application starts successfully ✅
- Logs show: "Failed to initialize MPU-6050 sensor..."
- `/gyro/status` returns `available: false`
- All `/gyro/*` endpoints return HTTP 503
- All `/gpio/*` and `/rotate/*` endpoints work normally ✅

### Scenario 2: Sensor Not Connected

**Setup:**
- Physically disconnect MPU-6050 from I2C pins
- I2C interface is enabled

**Expected Behavior:**
- Application starts successfully ✅
- Logs show: "Failed to initialize MPU-6050 sensor... Error 5..."
- `/gyro/status` returns `available: false`
- All `/gyro/*` endpoints return HTTP 503
- LED and servo work normally ✅

### Scenario 3: Wrong I2C Address

**Setup:**
- Sensor is connected to different I2C address (e.g., 0x69)
- Code tries to access 0x68

**Expected Behavior:**
- Application starts successfully ✅
- Sensor marked as unavailable
- Gyro endpoints return 503
- System continues functioning

### Scenario 4: Sensor Properly Connected

**Setup:**
- MPU-6050 connected to I2C bus 1, address 0x68
- I2C interface enabled
- Proper wiring: SDA → GPIO 2, SCL → GPIO 3, VCC → 3.3V, GND → GND

**Expected Behavior:**
- Application starts successfully ✅
- Logs show: "MPU-6050 initialized successfully"
- `/gyro/status` returns `available: true`
- All `/gyro/*` endpoints return sensor data ✅
- LED and servo work normally ✅

## Log Monitoring

### View logs in real-time:
```bash
dotnet run | grep -E "MPU|gyro|Sensor"
```

### Expected log patterns:

**Success case:**
```
[INF] Attempting to initialize MPU-6050 sensor on I2C bus 1, address 0x68...
[INF] MPU-6050 initialized successfully (PWR_MGMT_1 set to 0).
```

**Failure case:**
```
[ERR] Failed to initialize MPU-6050 sensor. This could be due to: I2C not enabled, 
sensor not connected, wrong address, or permission issues. Gyro endpoints will 
return 503 errors until the sensor is available.
System.IO.IOException: Error 5 performing I2C data transfer.
```

**Runtime warning (when sensor unavailable):**
```
[WRN] Sensor unavailable: MPU-6050 sensor is not available. Last error: Error 5...
```

## Integration Testing Checklist

- [ ] Application starts without MPU-6050 connected
- [ ] `/gyro/status` correctly reports availability
- [ ] Gyro endpoints return 503 when sensor unavailable
- [ ] Gyro endpoints return data when sensor available
- [ ] LED endpoints work regardless of sensor status
- [ ] Servo endpoints work regardless of sensor status
- [ ] `/health` endpoint works
- [ ] No unhandled exceptions in logs
- [ ] Clean shutdown with Ctrl+C

## Browser Testing

### Open in browser:
```
http://<raspberry-pi-ip>:5000
```

### Test UI interaction:
1. Click LED buttons → Should work ✅
2. Move servo slider → Should work ✅
3. Click gyro buttons:
   - If sensor connected → Should trigger routines ✅
   - If sensor disconnected → Should show error message in status area

## Troubleshooting

### If logs show "Error 5":
- Check I2C is enabled: `sudo raspi-config`
- Verify wiring connections
- Check I2C bus: `sudo i2cdetect -y 1`
- Verify user permissions: `sudo usermod -a -G i2c $USER`

### If application won't start:
- Check port 5000 is available: `sudo netstat -tulpn | grep 5000`
- Verify .NET 8.0 is installed: `dotnet --version`
- Check file permissions in project directory

### If sensor shows available but returns bad data:
- Sensor might be damaged
- Check power supply (3.3V, not 5V)
- Verify I2C pull-up resistors are present

## Success Criteria

✅ Application never crashes due to MPU-6050 errors
✅ Clear error messages in logs and API responses
✅ LED and servo functionality independent of sensor
✅ Graceful degradation when sensor unavailable
✅ Easy debugging with `/gyro/status` endpoint
✅ Proper HTTP status codes (503 for unavailable, 500 for unexpected)
✅ All errors logged with appropriate severity
