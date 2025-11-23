using System;
using System.Device.Pwm;
using System.Device.Pwm.Drivers;
using System.Threading.Tasks;
using Iot.Device.ServoMotor;

namespace ConsoleApp1.Hardware;

/// <summary>
/// Encapsulates servo initialization and movement routines.
/// </summary>
public class ServoController : IDisposable
{
    private readonly HardwareLocks _locks;
    private readonly int _pin;
    private readonly Random _rand = new();
    private SoftwarePwmChannel? _pwm;
    private ServoMotor? _servo;

    public int CurrentAngle { get; private set; } = 90; // rest angle

    public ServoController(HardwareLocks locks, int servoPin = 18)
    {
        _locks = locks;
        _pin = servoPin;
    }

    private void EnsureInitialized()
    {
        if (_servo != null) return;
        _pwm = new SoftwarePwmChannel(_pin, frequency:50, dutyCycle:0.05, usePrecisionTimer:true);
        _pwm.Start();
        _servo = new ServoMotor(_pwm, minimumPulseWidthMicroseconds:500, maximumPulseWidthMicroseconds:2500);
        _servo.Start();
        WriteAngle(90);
        Console.WriteLine($"Servo initialized on GPIO pin {_pin}.");
    }

    /// <summary>Writes servo angle with clamping and tracking.</summary>
    public void WriteAngle(int angle)
    {
        EnsureInitialized();
        angle = Math.Clamp(angle,0,180);
        _servo!.WriteAngle(angle);
        CurrentAngle = angle;
    }

    /// <summary>Rotate servo asynchronously to a target angle.</summary>
    public async Task RotateAsync(int angle)
    {
        await _locks.ServoLock.WaitAsync();
        try
        {
            WriteAngle(angle);
            await Task.Delay(500);
        }
        finally { _locks.ServoLock.Release(); }
    }

    #region Routines (ported from original Program.cs)

    public async Task WaveAsync(int cycles = 5, int delayMs = 90)
    {
        await _locks.ServoLock.WaitAsync();
        try
        {
            EnsureInitialized();
            WriteAngle(0);
            await Task.Delay(delayMs);
            for (int i=0;i<cycles;i++)
            {
                WriteAngle(180);
                await Task.Delay(delayMs);
                WriteAngle(0);
                await Task.Delay(delayMs);
            }
        }
        finally { _locks.ServoLock.Release(); }
    }

    public async Task SmoothSweepAsync()
    {
        await _locks.ServoLock.WaitAsync();
        try
        {
            EnsureInitialized();
            for (int a=0;a<=180;a++){ WriteAngle(a); await Task.Delay(12); }
            for (int a=180;a>=0;a--){ WriteAngle(a); await Task.Delay(12); }
        }
        finally { _locks.ServoLock.Release(); }
    }

    public async Task RandomDanceAsync(int moves = 15)
    {
        await _locks.ServoLock.WaitAsync();
        try
        {
            EnsureInitialized();
            for (int i=0;i<moves;i++)
            {
                WriteAngle(_rand.Next(0,181));
                await Task.Delay(_rand.Next(120,260));
            }
        }
        finally { _locks.ServoLock.Release(); }
    }

    public async Task JitterAsync()
    {
        await _locks.ServoLock.WaitAsync();
        try
        {
            EnsureInitialized();
            int center = 90;
            int[] offsets = { -25, 25, -40, 40, -15, 15, -5, 5, 0 };
            for (int repeat=0; repeat<3; repeat++)
            {
                foreach (int offset in offsets)
                {
                    WriteAngle(Math.Clamp(center+offset,0,180));
                    await Task.Delay(70);
                }
            }
        }
        finally { _locks.ServoLock.Release(); }
    }

    public async Task PoseSequenceAsync()
    {
        await _locks.ServoLock.WaitAsync();
        try
        {
            EnsureInitialized();
            int[] seq = {0,60,120,180,120,60,0,90};
            foreach (int a in seq){ WriteAngle(a); await Task.Delay(220);}    
        }
        finally { _locks.ServoLock.Release(); }
    }

    public async Task MicroSweepAsync()
    {
        await _locks.ServoLock.WaitAsync();
        try
        {
            EnsureInitialized();
            for (int cycle=0; cycle<3; cycle++)
            {
                for (int a=60;a<=120;a+=2){ WriteAngle(a); await Task.Delay(25);}    
                for (int a=120;a>=60;a-=2){ WriteAngle(a); await Task.Delay(25);}  
            }
            WriteAngle(90);
        }
        finally { _locks.ServoLock.Release(); }
    }

    public async Task SlowPanAsync()
    {
        await _locks.ServoLock.WaitAsync();
        try
        {
            EnsureInitialized();
            for (int a=0;a<=180;a+=5){ WriteAngle(a); await Task.Delay(80);}    
        }
        finally { _locks.ServoLock.Release(); }
    }

    public async Task DoubleWaveAsync()
    {
        await _locks.ServoLock.WaitAsync();
        try
        {
            EnsureInitialized();
            for (int cycle=0; cycle<2; cycle++)
            {
                for (int a=0;a<=180;a+=10){ WriteAngle(a); await Task.Delay(40);}   
                for (int a=180;a>=0;a-=10){ WriteAngle(a); await Task.Delay(40);}  
            }
        }
        finally { _locks.ServoLock.Release(); }
    }

    public async Task FocusSweepAsync()
    {
        await _locks.ServoLock.WaitAsync();
        try
        {
            EnsureInitialized();
            int center = 90;
            for (int amplitude=60; amplitude>=15; amplitude-=15)
            {
                WriteAngle(Math.Clamp(center - amplitude,0,180)); await Task.Delay(110);
                WriteAngle(Math.Clamp(center + amplitude,0,180)); await Task.Delay(110);
            }
            WriteAngle(center);
        }
        finally { _locks.ServoLock.Release(); }
    }

    public async Task SineRideAsync()
    {
        await _locks.ServoLock.WaitAsync();
        try
        {
            EnsureInitialized();
            for (int deg=0; deg<=360; deg+=15)
            {
                int angle = (int)Math.Clamp(90 + 90*Math.Sin(deg*Math.PI/180.0),0,180);
                WriteAngle(angle); await Task.Delay(45);
            }
            for (int deg=360; deg>=0; deg-=15)
            {
                int angle = (int)Math.Clamp(90 + 90*Math.Sin(deg*Math.PI/180.0),0,180);
                WriteAngle(angle); await Task.Delay(45);
            }
            WriteAngle(90);
        }
        finally { _locks.ServoLock.Release(); }
    }

    public async Task RandomPauseAsync()
    {
        await _locks.ServoLock.WaitAsync();
        try
        {
            EnsureInitialized();
            for (int i=0;i<12;i++)
            {
                WriteAngle(_rand.Next(0,181));
                await Task.Delay(_rand.Next(180,520));
            }
        }
        finally { _locks.ServoLock.Release(); }
    }

    public async Task EdgePulseAsync()
    {
        await _locks.ServoLock.WaitAsync();
        try
        {
            EnsureInitialized();
            for (int cycle=0; cycle<4; cycle++)
            {
                WriteAngle(5); await Task.Delay(120);
                WriteAngle(175); await Task.Delay(120);
                WriteAngle(10); await Task.Delay(120);
                WriteAngle(170); await Task.Delay(120);
            }
            WriteAngle(90);
        }
        finally { _locks.ServoLock.Release(); }
    }

    public async Task DrumRollAsync()
    {
        await _locks.ServoLock.WaitAsync();
        try
        {
            EnsureInitialized();
            int baseAngle = 30;
            for (int cycle=0; cycle<3; cycle++)
            {
                for (int i=0;i<18;i++)
                {
                    WriteAngle(baseAngle + (i % 2 == 0 ? 20 : -20));
                    await Task.Delay(35);
                }
                baseAngle += 30;
            }
            WriteAngle(90);
        }
        finally { _locks.ServoLock.Release(); }
    }

    public async Task SpinCycleAsync()
    {
        await _locks.ServoLock.WaitAsync();
        try
        {
            EnsureInitialized();
            for (int cycle=0; cycle<4; cycle++)
            {
                for (int a=0;a<=180;a+=6){ WriteAngle(a); await Task.Delay(22);}  
                for (int a=180;a>=0;a-=6){ WriteAngle(a); await Task.Delay(22);} 
            }
            WriteAngle(90);
        }
        finally { _locks.ServoLock.Release(); }
    }

    public async Task HeartbeatArcAsync()
    {
        await _locks.ServoLock.WaitAsync();
        try
        {
            EnsureInitialized();
            for (int beat=0; beat<4; beat++)
            {
                WriteAngle(120); await Task.Delay(90);
                WriteAngle(150); await Task.Delay(70);
                WriteAngle(110); await Task.Delay(150);
                WriteAngle(90); await Task.Delay(220);
            }
        }
        finally { _locks.ServoLock.Release(); }
    }

    public async Task RainDropAsync()
    {
        await _locks.ServoLock.WaitAsync();
        try
        {
            EnsureInitialized();
            int center = 90;
            int[] amps = {10,25,45,15,30,50,20};
            foreach (int amp in amps)
            {
                WriteAngle(Math.Clamp(center+amp,0,180)); await Task.Delay(130);
                WriteAngle(Math.Clamp(center - amp/2,0,180)); await Task.Delay(130);
            }
            WriteAngle(center);
        }
        finally { _locks.ServoLock.Release(); }
    }

    #endregion

    public void Dispose()
    {
        _servo?.Dispose();
        _pwm?.Dispose();
    }
}
