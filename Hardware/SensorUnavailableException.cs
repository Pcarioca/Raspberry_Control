using System;

namespace ConsoleApp1.Hardware;

/// <summary>
/// Exception thrown when MPU-6050 sensor is unavailable or not responding.
/// </summary>
public class SensorUnavailableException : Exception
{
    public SensorUnavailableException(string message) : base(message) { }
    
    public SensorUnavailableException(string message, Exception innerException) 
        : base(message, innerException) { }
}
