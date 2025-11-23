namespace ConsoleApp1.Models;

/// <summary>
/// Raw IMU data (scaled physical units): acceleration in g, gyro in deg/s.
/// </summary>
public record RawImuData(double Ax, double Ay, double Az, double Gx, double Gy, double Gz);
