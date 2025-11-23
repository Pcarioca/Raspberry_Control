using ConsoleApp1.Models;
using System;

namespace ConsoleApp1.Utils;

/// <summary>
/// Static helpers for converting raw accelerometer readings to pitch/roll.
/// </summary>
public static class AngleMath
{
    public static AngleData ComputeAccelAngles(double ax, double ay, double az)
    {
        // Basic formulas (assuming small angles). Pitch from X/Z, Roll from Y/Z
        // Using atan2 for numerical stability.
        double pitch = Math.Atan2(ay, Math.Sqrt(ax*ax + az*az)) * 180.0 / Math.PI;
        double roll  = Math.Atan2(-ax, az) * 180.0 / Math.PI;
        return new AngleData(pitch, roll, 0);
    }
}
