namespace ConsoleApp1.Models;

/// <summary>
/// Standard JSON response for gyro-driven fun routines.
/// </summary>
public record GyroRoutineResult(string Name, bool Ok, string Description, object? Data = null);
