using System;

namespace ConsoleApp1.Utils;

/// <summary>
/// Provides simple filtering utilities for IMU data (e.g., complementary filter).
/// </summary>
public class MotionFilters
{
    private double _pitch;
    private double _roll;
    private DateTime _last = DateTime.UtcNow;

    /// <summary>
    /// Complementary filter fusing new accel pitch/roll with integrated gyro delta.
    /// gyroPitchRate / gyroRollRate are deg/s about X and Y respectively.
    /// </summary>
    public (double Pitch, double Roll) Complementary(double accelPitch, double accelRoll, double gyroPitchRate, double gyroRollRate, double alpha = 0.96)
    {
        var now = DateTime.UtcNow;
        double dt = (now - _last).TotalSeconds;
        _last = now;

        // Integrate gyro rates
        _pitch += gyroPitchRate * dt;
        _roll  += gyroRollRate * dt;

        // Fuse: high-pass gyro integrated angle + low-pass accel absolute
        _pitch = alpha * _pitch + (1 - alpha) * accelPitch;
        _roll  = alpha * _roll  + (1 - alpha) * accelRoll;

        return (_pitch, _roll);
    }
}
