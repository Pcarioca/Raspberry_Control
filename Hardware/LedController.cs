using System;
using System.Device.Gpio;
using System.Threading.Tasks;

namespace ConsoleApp1.Hardware;

/// <summary>
/// LED control and pattern routines.
/// </summary>
public class LedController : IDisposable
{
    private readonly HardwareLocks _locks;
    private readonly int _pin;
    private readonly GpioController _gpio = new();
    private readonly Random _rand = new();

    private DateTime _lastPattern = DateTime.MinValue;
    private readonly TimeSpan _throttleWindow = TimeSpan.FromMilliseconds(200);

    public bool IsOn { get; private set; }

    public LedController(HardwareLocks locks, int ledPin = 17)
    {
        _locks = locks;
        _pin = ledPin;
        _gpio.OpenPin(_pin, PinMode.Output);
        _gpio.Write(_pin, PinValue.Low);
        IsOn = false;
        Console.WriteLine($"LED initialized on GPIO pin {_pin}.");
    }

    public void TurnOn()
    {
        _gpio.Write(_pin, PinValue.High);
        IsOn = true;
    }

    public void TurnOff()
    {
        _gpio.Write(_pin, PinValue.Low);
        IsOn = false;
    }

    public bool Throttled()
    {
        var now = DateTime.UtcNow;
        if (now - _lastPattern < _throttleWindow) return true;
        _lastPattern = now; return false;
    }

    private void Write(bool high) => _gpio.Write(_pin, high ? PinValue.High : PinValue.Low);

    public async Task StrobeAsync()
    {
        await _locks.LedLock.WaitAsync();
        try
        {
            Console.WriteLine("LED strobe start.");
            for (int i=0;i<20;i++){ Write(true); await Task.Delay(60); Write(false); await Task.Delay(60);}    
            Console.WriteLine("LED strobe end.");
        }
        finally { _locks.LedLock.Release(); }
    }

    public async Task PulseAsync()
    {
        await _locks.LedLock.WaitAsync();
        try
        {
            int[] widths = {80,140,220,320,220,140,80};
            foreach (int w in widths){ Write(true); await Task.Delay(w); Write(false); await Task.Delay(120);}   
        }
        finally { _locks.LedLock.Release(); }
    }

    public async Task SosAsync()
    {
        await _locks.LedLock.WaitAsync();
        try
        {
            const int dot=150; const int dash=dot*3; const int gap=dot;
            async Task Emit(int dur){ Write(true); await Task.Delay(dur); Write(false); await Task.Delay(gap);}    
            // S
            await Emit(dot); await Emit(dot); await Emit(dot);
            await Task.Delay(dash);
            // O
            await Emit(dash); await Emit(dash); await Emit(dash);
            await Task.Delay(dash);
            // S
            await Emit(dot); await Emit(dot); await Emit(dot);
        }
        finally { _locks.LedLock.Release(); }
    }

    public async Task BreathingAsync()
    {
        await _locks.LedLock.WaitAsync();
        try
        {
            int[] rise={20,40,60,80,100,120,140,160,180};
            int[] fall={180,160,140,120,100,80,60,40,20};
            for (int cycle=0; cycle<2; cycle++)
            {
                foreach (int on in rise){ Write(true); await Task.Delay(on); Write(false); await Task.Delay(200 - Math.Min(on,180)); }
                foreach (int on in fall){ Write(true); await Task.Delay(on); Write(false); await Task.Delay(200 - Math.Min(on,180)); }
            }
        }
        finally { _locks.LedLock.Release(); }
    }

    public async Task BlinkBurstAsync()
    {
        await _locks.LedLock.WaitAsync();
        try
        {
            for (int burst=0; burst<3; burst++)
            {
                for (int i=0;i<5;i++){ Write(true); await Task.Delay(70); Write(false); await Task.Delay(70);}  
                await Task.Delay(320);
            }
        }
        finally { _locks.LedLock.Release(); }
    }

    public async Task RandomSparkleAsync()
    {
        await _locks.LedLock.WaitAsync();
        try
        {
            for (int i=0;i<18;i++){ Write(true); await Task.Delay(_rand.Next(40,150)); Write(false); await Task.Delay(_rand.Next(90,260)); }
        }
        finally { _locks.LedLock.Release(); }
    }

    public async Task LongFlashAsync()
    {
        await _locks.LedLock.WaitAsync();
        try
        {
            for (int i=0;i<3;i++){ Write(true); await Task.Delay(700); Write(false); await Task.Delay(200);}  
        }
        finally { _locks.LedLock.Release(); }
    }

    public async Task TwinkleAsync()
    {
        await _locks.LedLock.WaitAsync();
        try
        {
            for (int i=0;i<12;i++){ Write(true); await Task.Delay(40 + 10*(i%4)); Write(false); await Task.Delay(140 + 20*(i%3)); }
        }
        finally { _locks.LedLock.Release(); }
    }

    public async Task PatternFlashAsync(int flashes, int onMs, int offMs)
    {
        await _locks.LedLock.WaitAsync();
        try
        {
            for (int i=0;i<flashes;i++){ Write(true); await Task.Delay(onMs); Write(false); await Task.Delay(offMs);}  
        }
        finally { _locks.LedLock.Release(); }
    }

    public void Dispose()
    {
        if (_gpio.IsPinOpen(_pin)) { Write(false); _gpio.ClosePin(_pin); }
        _gpio.Dispose();
    }
}
