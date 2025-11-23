using System;
using System.Device.I2c;
using System.Numerics;
using System.Threading.Tasks;
using ConsoleApp1.Hardware;
using ConsoleApp1.Models;
using ConsoleApp1.Utils;
using Iot.Device.Imu;
using Microsoft.Extensions.Logging;

namespace ConsoleApp1.Hardware;

/// <summary>
/// MPU-6050 access layer: handles initialization and cached sensor readings.
/// Provides raw acceleration & gyro plus computed angles.
/// Resilient design: sensor unavailability does not crash the application.
/// Uses the official Iot.Device.Imu.Mpu6050 driver internally.
/// </summary>
public class Mpu6050 : IDisposable
{
    private readonly HardwareLocks _locks;
    private readonly ILogger<Mpu6050> _logger;
    private readonly MotionFilters _filters = new();

    private I2cDevice? _i2cDevice;
    private Iot.Device.Imu.Mpu6050? _sensor;

    private RawImuData _lastRaw = new(0,0,0,0,0,0);
    private AngleData _lastAngles = new(0,0,0);
    private DateTime _lastUpdate = DateTime.MinValue;

    private bool _isInitialized;
    private bool _isAvailable = true;
    private Exception? _lastError;

    public Mpu6050(HardwareLocks locks, ILogger<Mpu6050> logger)
    {
        _locks = locks;
        _logger = logger;
        // Do NOT initialize here - use lazy initialization on first access
    }

    private void Initialize()
    {
        // Idempotent: if already initialized, just return
        if (_isInitialized)
            return;

        try
        {
            _logger.LogInformation("Attempting to initialize MPU-6050 sensor on I2C bus 1, address 0x68...");
            
            // Create I2C device connection using default address (0x68)
            var settings = new I2cConnectionSettings(1, Iot.Device.Imu.Mpu6050.DefaultI2cAddress);
            _i2cDevice = I2cDevice.Create(settings);

            // Create the official MPU-6050 driver instance
            _sensor = new Iot.Device.Imu.Mpu6050(_i2cDevice);

            // Configure sensor with reasonable defaults
            _sensor.AccelerometerRange = AccelerometerRange.Range02G;  // ±2g
            _sensor.GyroscopeRange = GyroscopeRange.Range0250Dps;      // ±250°/s
            
            // Set moderate bandwidths for balanced noise/latency
            _sensor.AccelerometerBandwidth = AccelerometerBandwidth.Bandwidth0184Hz;
            _sensor.GyroscopeBandwidth = GyroscopeBandwidth.Bandwidth0184Hz;

            _isInitialized = true;
            _isAvailable = true;
            _lastError = null;
            _logger.LogInformation("MPU-6050 initialized successfully with official driver.");
        }
        catch (Exception ex)
        {
            _isAvailable = false;
            _lastError = ex;
            _logger.LogError(ex, 
                "Failed to initialize MPU-6050 sensor. " +
                "This could be due to: I2C not enabled, sensor not connected, wrong address, or permission issues. " +
                "Gyro endpoints will return 503 errors until the sensor is available.");
            
            // Do NOT rethrow - gracefully mark as unavailable
        }
    }

    /// <summary>Check if sensor is available and responding.</summary>
    public bool IsAvailable => _isAvailable;

    /// <summary>Get last error message if sensor is unavailable.</summary>
    public string? GetLastErrorMessage() => _lastError?.Message;

    /// <summary>Ensures sensor is initialized before use. Throws SensorUnavailableException if not available.</summary>
    private void EnsureAvailable()
    {
        // Attempt lazy initialization
        Initialize();

        // Check availability
        if (!_isAvailable)
        {
            var message = $"MPU-6050 sensor is not available. Last error: {_lastError?.Message ?? "Unknown"}";
            if (_lastError != null)
                throw new SensorUnavailableException(message, _lastError);
            else
                throw new SensorUnavailableException(message);
        }
    }

    /// <summary>Reads raw sensor values (accel XYZ, gyro XYZ) from device.</summary>
    public async Task<RawImuData> ReadRawAsync()
    {
        EnsureAvailable(); // Throws if unavailable

        await _locks.ImuLock.WaitAsync();
        try
        {
            // Use the official driver to get accelerometer and gyroscope readings
            Vector3 accel = _sensor!.GetAccelerometer();  // Returns in g
            Vector3 gyro = _sensor.GetGyroscopeReading(); // Returns in deg/s

            // Map to our RawImuData model
            _lastRaw = new RawImuData(
                accel.X, accel.Y, accel.Z,  // Already in g
                gyro.X, gyro.Y, gyro.Z       // Already in deg/s
            );
            
            return _lastRaw;
        }
        catch (Exception ex)
        {
            // Mark sensor as unavailable on read failure
            _isAvailable = false;
            _lastError = ex;
            _logger.LogError(ex, "Failed to read from MPU-6050 sensor.");
            throw new SensorUnavailableException("Failed to read from MPU-6050.", ex);
        }
        finally 
        { 
            _locks.ImuLock.Release(); 
        }
    }

    /// <summary>Updates angle cache using complementary filter.</summary>
    public async Task<AngleData> UpdateAnglesAsync()
    {
        EnsureAvailable(); // Throws if unavailable
        
        try
        {
            var raw = await ReadRawAsync();
            var angles = AngleMath.ComputeAccelAngles(raw.Ax, raw.Ay, raw.Az);
            var fused = _filters.Complementary(angles.Pitch, angles.Roll, raw.Gx, raw.Gy);
            _lastAngles = new AngleData(fused.Pitch, fused.Roll, raw.Gz);
            _lastUpdate = DateTime.UtcNow;
            return _lastAngles;
        }
        catch (SensorUnavailableException)
        {
            // Already logged and marked unavailable in ReadRawAsync
            throw;
        }
        catch (Exception ex)
        {
            _isAvailable = false;
            _lastError = ex;
            _logger.LogError(ex, "Failed to update angles from MPU-6050.");
            throw new SensorUnavailableException("Failed to update angles from MPU-6050.", ex);
        }
    }

    public RawImuData LastRaw => _lastRaw;
    public AngleData LastAngles => _lastAngles;
    public DateTime LastUpdate => _lastUpdate;

    /// <summary>Magnitude of gyro vector (deg/s).</summary>
    public double GetGyroMagnitude()
    {
        EnsureAvailable(); // Throws if unavailable
        return Math.Sqrt(_lastRaw.Gx*_lastRaw.Gx + _lastRaw.Gy*_lastRaw.Gy + _lastRaw.Gz*_lastRaw.Gz);
    }

    /// <summary>Approx freefall detection: all accel components near 0g.</summary>
    public bool IsFreeFall(double thresholdG = 0.15)
    {
        EnsureAvailable(); // Throws if unavailable
        return Math.Abs(_lastRaw.Ax) < thresholdG && Math.Abs(_lastRaw.Ay) < thresholdG && Math.Abs(_lastRaw.Az) < thresholdG;
    }

    /// <summary>Stability check: gyro magnitude below threshold.</summary>
    public bool IsStable(double gyroThreshold = 5.0)
    {
        EnsureAvailable(); // Throws if unavailable
        return GetGyroMagnitude() < gyroThreshold;
    }

    public void Dispose()
    {
        _sensor?.Dispose();
        _i2cDevice?.Dispose();
    }
}
