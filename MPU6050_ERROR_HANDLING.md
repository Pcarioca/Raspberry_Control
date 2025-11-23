# MPU-6050 Resilient Error Handling

## Overview

This document describes the comprehensive error handling implemented for the MPU-6050 gyroscope sensor to prevent application crashes and provide graceful degradation when the sensor is unavailable.

## Problem Statement

Previously, if the MPU-6050 sensor was:
- Not physically connected
- Miswired
- I2C interface disabled on Raspberry Pi
- Bus busy or locked
- Wrong I2C address

The application would crash with an unhandled `System.IO.IOException` during service initialization, making the entire web server unavailable.

## Solution Architecture

### 1. Custom Exception Class

**File**: `Hardware/SensorUnavailableException.cs`

A dedicated exception type that clearly indicates sensor availability issues:

```csharp
public class SensorUnavailableException : Exception
```

This exception is caught at the API endpoint level and converted to proper HTTP error responses.

### 2. Lazy Initialization Pattern

**File**: `Hardware/Mpu6050.cs`

**Key Changes:**

- **Removed constructor initialization**: No I2C operations in constructor
- **Added state flags**:
  - `_isInitialized`: Tracks if initialization was attempted
  - `_isAvailable`: Indicates if sensor is working
  - `_lastError`: Stores initialization error for debugging

- **Idempotent `Initialize()` method**:
  - Only attempts initialization once
  - Wraps all I2C operations in try-catch
  - On failure: logs error, marks unavailable, does NOT throw
  - On success: marks initialized and available

- **`EnsureAvailable()` guard method**:
  - Called at the start of every public method
  - Attempts lazy initialization on first use
  - Throws `SensorUnavailableException` if sensor is unavailable
  - Prevents low-level I2C exceptions from bubbling up

### 3. Public Availability Check API

**New Public Methods in `Mpu6050`:**

```csharp
public bool IsAvailable => _isAvailable;
public string? GetLastErrorMessage() => _lastError?.Message;
```

These allow endpoints and diagnostic tools to check sensor status without triggering exceptions.

### 4. Endpoint Error Handling

**File**: `Api/GyroEndpoints.cs`

**All gyro endpoints now:**

1. Wrap sensor access in try-catch blocks
2. Catch `SensorUnavailableException` → return HTTP 503 (Service Unavailable)
3. Catch general `Exception` → return HTTP 500 (Internal Server Error)
4. Log all errors with appropriate severity
5. Return JSON problem details instead of crashing

**Example Pattern:**

```csharp
app.MapGet("/gyro/raw", (Mpu6050 imu, ILogger<Mpu6050> logger) =>
{
    try
    {
        return Results.Json(imu.LastRaw);
    }
    catch (SensorUnavailableException ex)
    {
        logger.LogWarning("Sensor unavailable: {Message}", ex.Message);
        return Results.Problem(
            title: "MPU-6050 sensor unavailable",
            detail: ex.Message,
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Unexpected error accessing MPU-6050");
        return Results.Problem(
            title: "Unexpected MPU-6050 error",
            detail: ex.Message,
            statusCode: StatusCodes.Status500InternalServerError);
    }
});
```

### 5. Diagnostic Endpoint

**New Endpoint**: `/gyro/status`

Returns sensor availability information:

```json
{
  "available": false,
  "lastError": "Error 5 performing I2C data transfer",
  "lastUpdate": "2025-11-23T10:30:00Z"
}
```

This helps debug sensor connection issues without crashing the app.

## Affected Endpoints

All gyro-related endpoints now have resilient error handling:

### Data Endpoints
- `/gyro/status` - NEW: Sensor health check
- `/gyro/raw` - Raw sensor readings
- `/gyro/angles` - Computed angles
- `/gyro/magnitude` - Gyro magnitude
- `/gyro/freefall` - Freefall detection
- `/gyro/stabilityCheck` - Stability check
- `/gyro/update` - Force sensor read

### Fun Routines (37 endpoints)
All `/gyro/fun/*` endpoints that depend on sensor data now handle errors gracefully.

## Behavior Summary

### When Sensor is Available ✅
- Application starts normally
- Gyro endpoints return sensor data
- LED and servo endpoints work normally

### When Sensor is Unavailable ⚠️
- Application still starts successfully
- LED and servo endpoints work normally (unaffected)
- Gyro endpoints return HTTP 503 with JSON error:
  ```json
  {
    "type": "https://tools.ietf.org/html/rfc9110#section-15.6.4",
    "title": "MPU-6050 sensor unavailable",
    "status": 503,
    "detail": "MPU-6050 sensor is not available. Last error: Error 5 performing I2C data transfer"
  }
  ```
- Errors logged to console with clear diagnostic information
- No application crashes or unhandled exceptions

## Logging

### Initialization Success
```
[INF] Attempting to initialize MPU-6050 sensor on I2C bus 1, address 0x68...
[INF] MPU-6050 initialized successfully (PWR_MGMT_1 set to 0).
```

### Initialization Failure
```
[ERR] Failed to initialize MPU-6050 sensor. This could be due to: I2C not enabled, 
sensor not connected, wrong address, or permission issues. Gyro endpoints will 
return 503 errors until the sensor is available.
System.IO.IOException: Error 5 performing I2C data transfer.
   at System.Device.I2c.UnixI2cBus.Write(...)
```

### Runtime Endpoint Access
```
[WRN] Sensor unavailable: MPU-6050 sensor is not available. Last error: Error 5...
```

## Testing Recommendations

### Test 1: Sensor Disconnected
1. Physically disconnect MPU-6050
2. Start application → Should start successfully
3. Access `/gyro/status` → Should show `available: false`
4. Access `/gyro/raw` → Should return HTTP 503
5. Access `/gpio/on` → Should work normally ✅

### Test 2: Sensor Connected
1. Connect MPU-6050 properly (I2C bus 1, address 0x68)
2. Start application → Should initialize sensor
3. Access `/gyro/status` → Should show `available: true`
4. Access `/gyro/raw` → Should return sensor data ✅

### Test 3: I2C Disabled
1. Disable I2C interface: `sudo raspi-config` → Interface Options → I2C → No
2. Start application → Should start with sensor unavailable
3. All gyro endpoints return 503
4. LED and servo endpoints work normally

## Migration Notes

### Dependency Injection
`Mpu6050` is registered as a singleton in `Program.cs`:

```csharp
builder.Services.AddSingleton<Mpu6050>();
```

This ensures only one instance exists and initialization is attempted once.

### Logger Injection
`Mpu6050` constructor now requires `ILogger<Mpu6050>`:

```csharp
public Mpu6050(HardwareLocks locks, ILogger<Mpu6050> logger)
```

The DI container automatically provides this.

## Future Enhancements

Potential improvements:
1. **Retry mechanism**: Periodically retry sensor initialization in background
2. **Health check endpoint**: Integrate with `/health` endpoint
3. **Metrics**: Track sensor availability uptime
4. **Alternative I2C addresses**: Try 0x69 if 0x68 fails
5. **Mock mode**: Provide simulated sensor data when unavailable

## Conclusion

The MPU-6050 sensor is now a fully resilient component that:
- ✅ Never crashes the application
- ✅ Provides clear error messages
- ✅ Logs diagnostic information
- ✅ Allows the rest of the system to function normally
- ✅ Supports easy debugging with `/gyro/status` endpoint

The application can now run successfully on any Raspberry Pi, regardless of whether the MPU-6050 sensor is connected or properly configured.
