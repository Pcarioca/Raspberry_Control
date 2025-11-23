namespace ConsoleApp1.Models;

/// <summary>
/// Computed orientation angles (Pitch, Roll) plus Yaw placeholder (from gyro Z integration placeholder).
/// </summary>
public record AngleData(double Pitch, double Roll, double Yaw);
