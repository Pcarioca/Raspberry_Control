using ConsoleApp1.Hardware;

namespace ConsoleApp1.Api;

/// <summary>Maps combo routines using both servo & LED.</summary>
public static class ComboEndpoints
{
    public static void MapComboEndpoints(this WebApplication app)
    {
        app.MapGet("/combo/party", async (ServoController servo, LedController led, HardwareLocks locks) =>
        {
            await locks.ComboLock.WaitAsync();
            try
            {
                for (int i=0;i<16;i++)
                {
                    servo.WriteAngle(new Random().Next(0,181));
                    led.TurnOn(); await Task.Delay(90); led.TurnOff(); await Task.Delay(90);
                }
            }
            finally { locks.ComboLock.Release(); }
            return Results.Json(new { ok=true, combo="party" });
        });

        app.MapGet("/combo/countdown", async (ServoController servo, LedController led, HardwareLocks locks) =>
        {
            await locks.ComboLock.WaitAsync();
            try
            {
                int[] positions = {180,120,60,0};
                foreach (var a in positions){ servo.WriteAngle(a); led.TurnOn(); await Task.Delay(350); led.TurnOff(); await Task.Delay(220);}    
                for (int i=0;i<5;i++){ led.TurnOn(); await Task.Delay(80); led.TurnOff(); await Task.Delay(80);}  
                servo.WriteAngle(90);
            }
            finally { locks.ComboLock.Release(); }
            return Results.Json(new { ok=true, combo="countdown" });
        });

        app.MapGet("/combo/alarm", async (ServoController servo, LedController led, HardwareLocks locks) =>
        {
            await locks.ComboLock.WaitAsync();
            try
            {
                for (int i=0;i<8;i++)
                {
                    servo.WriteAngle(i % 2 == 0 ? 20 : 160);
                    led.TurnOn(); await Task.Delay(160); led.TurnOff(); await Task.Delay(140);
                }
                servo.WriteAngle(90);
            }
            finally { locks.ComboLock.Release(); }
            return Results.Json(new { ok=true, combo="alarm" });
        });

        app.MapGet("/combo/celebration", async (ServoController servo, LedController led, HardwareLocks locks) =>
        {
            await locks.ComboLock.WaitAsync();
            try
            {
                var rand = new Random();
                for (int i=0;i<10;i++)
                {
                    servo.WriteAngle(rand.Next(0,181));
                    led.TurnOn(); await Task.Delay(60 + (i % 3)*20); led.TurnOff(); await Task.Delay(60 + ((i+1)%3)*20);
                }
            }
            finally { locks.ComboLock.Release(); }
            return Results.Json(new { ok=true, combo="celebration" });
        });

        app.MapGet("/combo/pulse-sync", async (ServoController servo, LedController led, HardwareLocks locks) =>
        {
            await locks.ComboLock.WaitAsync();
            try
            {
                int[] tempo = {90,60,120,60};
                for (int cycle=0; cycle<4; cycle++)
                {
                    servo.WriteAngle(80); led.TurnOn(); await Task.Delay(tempo[cycle%tempo.Length]);
                    servo.WriteAngle(100); await Task.Delay(tempo[cycle%tempo.Length]);
                    led.TurnOff(); await Task.Delay(tempo[cycle%tempo.Length] + 80);
                }
                servo.WriteAngle(90);
            }
            finally { locks.ComboLock.Release(); }
            return Results.Json(new { ok=true, combo="pulse-sync" });
        });

        app.MapGet("/combo/guard-sweep", async (ServoController servo, LedController led, HardwareLocks locks) =>
        {
            await locks.ComboLock.WaitAsync();
            try
            {
                int[] points = {20,60,120,160,120,60,20};
                foreach (var a in points){ servo.WriteAngle(a); led.TurnOn(); await Task.Delay(260); led.TurnOff(); await Task.Delay(180);}   
                servo.WriteAngle(90);
            }
            finally { locks.ComboLock.Release(); }
            return Results.Json(new { ok=true, combo="guard-sweep" });
        });

        app.MapGet("/combo/random-show", async (ServoController servo, LedController led, HardwareLocks locks) =>
        {
            await locks.ComboLock.WaitAsync();
            try
            {
                var rand = new Random();
                for (int i=0;i<12;i++)
                {
                    servo.WriteAngle(rand.Next(0,181));
                    if (rand.NextDouble()>0.5) led.TurnOn(); else led.TurnOff();
                    await Task.Delay(rand.Next(120,360));
                }
                led.TurnOff(); servo.WriteAngle(90);
            }
            finally { locks.ComboLock.Release(); }
            return Results.Json(new { ok=true, combo="random-show" });
        });
    }
}
