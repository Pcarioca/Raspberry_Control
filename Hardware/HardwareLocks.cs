using System.Threading;

namespace ConsoleApp1.Hardware;

/// <summary>
/// Centralized lock/semaphore management to avoid cross-module deadlocks.
/// All hardware operations should acquire the relevant semaphore before touching GPIO/I2C.
/// </summary>
public class HardwareLocks
{
    /// <summary>Lock for servo movements to prevent overlapping PWM updates.</summary>
    public SemaphoreSlim ServoLock { get; } = new(1,1);
    /// <summary>Lock for LED patterns to avoid interleaving patterns.</summary>
    public SemaphoreSlim LedLock { get; } = new(1,1);
    /// <summary>Lock for routines using both servo and LED simultaneously.</summary>
    public SemaphoreSlim ComboLock { get; } = new(1,1);
    /// <summary>Lock for MPU-6050 sensor reads to keep I2C transactions consistent.</summary>
    public SemaphoreSlim ImuLock { get; } = new(1,1);
}
