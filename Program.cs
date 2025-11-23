/* LEGACY MONOLITH START (commented out by refactor)
using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Device.Gpio;
using System.Device.Pwm;
using System.Device.Pwm.Drivers;
using System.Threading;
using ConsoleApp1;
using Iot.Device.ServoMotor;

var builder = WebApplication.CreateBuilder(args);

// Listen on all interfaces
builder.WebHost.UseUrls("http://0.0.0.0:5000");

var app = builder.Build();

// Enable static file serving (e.g. from /static or wwwroot if added later)
app.UseStaticFiles();
using ConsoleApp1.Api;
using ConsoleApp1.Hardware;
using ConsoleApp1.Control;
using ConsoleApp1.Models;
using ConsoleApp1;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://0.0.0.0:5000");

// Dependency Injection registrations
builder.Services.AddSingleton<HardwareLocks>();
builder.Services.AddSingleton<ServoController>();
builder.Services.AddSingleton<LedController>();
builder.Services.AddSingleton<Mpu6050>();
builder.Services.AddSingleton<PidController>(); // placeholder for future PID

var app = builder.Build();
app.UseStaticFiles();

// Root route serving static HTML UI
app.MapGet("/", () =>
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Hosting;
    using ConsoleApp1.Api;
    using ConsoleApp1.Hardware;
    using ConsoleApp1.Control;

    var builder = WebApplication.CreateBuilder(args);
    builder.WebHost.UseUrls("http://0.0.0.0:5000");

    // Dependency Injection registrations
    builder.Services.AddSingleton<HardwareLocks>();
    builder.Services.AddSingleton<ServoController>();
    builder.Services.AddSingleton<LedController>();
    builder.Services.AddSingleton<Mpu6050>();
    builder.Services.AddSingleton<PidController>(); // placeholder for future PID

    var app = builder.Build();
    app.UseStaticFiles();

    // Root route serving static HTML UI
    app.MapGet("/", () =>
    {
        var htmlContent = HtmlLoader.LoadHtml("index.html");
        return Results.Content(htmlContent, "text/html");
    });

    // Map modular endpoint groups
    app.MapServoEndpoints();
    app.MapLedEndpoints();
    app.MapComboEndpoints();
    app.MapGyroEndpoints();
    app.MapPidEndpoints();

    // Health and system state endpoints
    app.MapGet("/health", (ServoController servo, LedController led, Mpu6050 imu) => Results.Json(new { status = "ok", servoReady = true, ledReady = true, imuReady = true }));
    app.MapGet("/state", (ServoController servo, LedController led, Mpu6050 imu) => Results.Json(new { servoAngle = servo.CurrentAngle, ledOn = led.IsOn, imuRaw = imu.LastRaw, imuAngles = imu.LastAngles }));

    // Numbers echo endpoints
    app.MapGet("/numbers", (int one, int two, int three) => Results.Json(new { one, two, three }));
    app.MapPost("/numbers", (NumbersPayload payload) => Results.Json(new { payload }));

    app.Run();

    public record NumbersPayload(int One, int Two, int Three);
    await WithServo(async s =>
    {
        Console.WriteLine("Random pause routine start.");
        for (int i = 0; i < 12; i++)
        {
            int nextAngle = random.Next(0, 181);
            int dwell = random.Next(180, 520);
            s.WriteAngle(nextAngle);
            await Task.Delay(dwell);
        }
        Console.WriteLine("Random pause routine complete.");
    });
}

async Task servoEdgePulse()
{
    await WithServo(async s =>
    {
        Console.WriteLine("Edge pulse start.");
        for (int cycle = 0; cycle < 4; cycle++)
        {
            s.WriteAngle(5);
            await Task.Delay(120);
            s.WriteAngle(175);
            await Task.Delay(120);
            s.WriteAngle(10);
            await Task.Delay(120);
            s.WriteAngle(170);
            await Task.Delay(120);
        }
        s.WriteAngle(90);
        Console.WriteLine("Edge pulse complete.");
    });
}

async Task servoDrumRoll()
{
    await WithServo(async s =>
    {
        Console.WriteLine("Drum roll start.");
        int baseAngle = 30;
        for (int cycle = 0; cycle < 3; cycle++)
        {
            for (int i = 0; i < 18; i++)
            {
                s.WriteAngle(baseAngle + (i % 2 == 0 ? 20 : -20));
                await Task.Delay(35);
            }
            baseAngle += 30;
        }
        s.WriteAngle(90);
        Console.WriteLine("Drum roll complete.");
    });
}

async Task servoSpinCycle()
{
    await WithServo(async s =>
    {
        Console.WriteLine("Spin cycle start.");
        for (int cycle = 0; cycle < 4; cycle++)
        {
            for (int angle = 0; angle <= 180; angle += 6)
            {
                s.WriteAngle(angle);
                await Task.Delay(22);
            }
            for (int angle = 180; angle >= 0; angle -= 6)
            {
                s.WriteAngle(angle);
                await Task.Delay(22);
            }
        }
        s.WriteAngle(90);
        Console.WriteLine("Spin cycle complete.");
    });
}

async Task servoHeartbeatArc()
{
    await WithServo(async s =>
    {
        Console.WriteLine("Heartbeat arc start.");
        for (int beat = 0; beat < 4; beat++)
        {
            s.WriteAngle(120);
            await Task.Delay(90);
            s.WriteAngle(150);
            await Task.Delay(70);
            s.WriteAngle(110);
            await Task.Delay(150);
            s.WriteAngle(90);
            await Task.Delay(220);
        }
        Console.WriteLine("Heartbeat arc complete.");
    });
}

async Task servoRainDrop()
{
    await WithServo(async s =>
    {
        Console.WriteLine("Raindrop routine start.");
        int center = 90;
        int[] amplitudes = { 10, 25, 45, 15, 30, 50, 20 };
        foreach (int amplitude in amplitudes)
        {
            s.WriteAngle(Math.Clamp(center + amplitude, 0, 180));
            await Task.Delay(130);
            s.WriteAngle(Math.Clamp(center - amplitude / 2, 0, 180));
            await Task.Delay(130);
        }
        s.WriteAngle(center);
        Console.WriteLine("Raindrop routine complete.");
    });
}

async Task ledStrobe()
{
    await WithLed(async () =>
    {
        Console.WriteLine("LED strobe start.");
        for (int i = 0; i < 20; i++)
        {
            gpioController.Write(ledPin, PinValue.High);
            await Task.Delay(60);
            gpioController.Write(ledPin, PinValue.Low);
            await Task.Delay(60);
        }
        Console.WriteLine("LED strobe complete.");
    });
}

async Task ledPulse()
{
    await WithLed(async () =>
    {
        Console.WriteLine("LED pulse start.");
        int[] widths = { 80, 140, 220, 320, 220, 140, 80 };
        foreach (int width in widths)
        {
            gpioController.Write(ledPin, PinValue.High);
            await Task.Delay(width);
            gpioController.Write(ledPin, PinValue.Low);
            await Task.Delay(120);
        }
        Console.WriteLine("LED pulse complete.");
    });
}

async Task ledSos()
{
    await WithLed(async () =>
    {
        Console.WriteLine("LED SOS start.");
        const int dot = 150;
        const int dash = dot * 3;
        const int gap = dot;

        async Task Emit(int duration)
        {
            gpioController.Write(ledPin, PinValue.High);
            await Task.Delay(duration);
            gpioController.Write(ledPin, PinValue.Low);
            await Task.Delay(gap);
        }

        // S
        await Emit(dot);
        await Emit(dot);
        await Emit(dot);

        await Task.Delay(dash); // letter gap

        // O
        await Emit(dash);
        await Emit(dash);
        await Emit(dash);

        await Task.Delay(dash); // letter gap

        // S
        await Emit(dot);
        await Emit(dot);
        await Emit(dot);

        Console.WriteLine("LED SOS complete.");
    });
}

async Task ledBreathing()
{
    await WithLed(async () =>
    {
        Console.WriteLine("LED breathing start.");
        // Simple software PWM style breathing using variable on/off times
        int[] rise = { 20, 40, 60, 80, 100, 120, 140, 160, 180 };
        int[] fall = rise.Reverse().ToArray();
        for (int cycle = 0; cycle < 3; cycle++)
        {
            foreach (int onMs in rise)
            {
                gpioController.Write(ledPin, PinValue.High);
                await Task.Delay(onMs);
                gpioController.Write(ledPin, PinValue.Low);
                await Task.Delay(200 - Math.Min(onMs, 180));
            }
            foreach (int onMs in fall)
            {
                gpioController.Write(ledPin, PinValue.High);
                await Task.Delay(onMs);
                gpioController.Write(ledPin, PinValue.Low);
                await Task.Delay(200 - Math.Min(onMs, 180));
            }
        }
        Console.WriteLine("LED breathing complete.");
    });
}

async Task ledBlinkBurst()
{
    await WithLed(async () =>
    {
        Console.WriteLine("LED blink burst start.");
        for (int burst = 0; burst < 3; burst++)
        {
            for (int i = 0; i < 5; i++)
            {
                gpioController.Write(ledPin, PinValue.High);
                await Task.Delay(70);
                gpioController.Write(ledPin, PinValue.Low);
                await Task.Delay(70);
            }
            await Task.Delay(320);
        }
        Console.WriteLine("LED blink burst complete.");
    });
}

async Task ledRandomSparkle()
{
    await WithLed(async () =>
    {
        Console.WriteLine("LED random sparkle start.");
        for (int i = 0; i < 18; i++)
        {
            gpioController.Write(ledPin, PinValue.High);
            await Task.Delay(random.Next(40, 150));
            gpioController.Write(ledPin, PinValue.Low);
            await Task.Delay(random.Next(90, 260));
        }
        Console.WriteLine("LED random sparkle complete.");
    });
}

async Task ledLongFlash()
{
    await WithLed(async () =>
    {
        Console.WriteLine("LED long flash start.");
        for (int i = 0; i < 3; i++)
        {
            gpioController.Write(ledPin, PinValue.High);
            await Task.Delay(700);
            gpioController.Write(ledPin, PinValue.Low);
            await Task.Delay(200);
        }
        Console.WriteLine("LED long flash complete.");
    });
}

async Task ledTwinkle()
{
    await WithLed(async () =>
    {
        Console.WriteLine("LED twinkle start.");
        for (int i = 0; i < 12; i++)
        {
            gpioController.Write(ledPin, PinValue.High);
            await Task.Delay(40 + 10 * (i % 4));
            gpioController.Write(ledPin, PinValue.Low);
            await Task.Delay(140 + 20 * (i % 3));
        }
        Console.WriteLine("LED twinkle complete.");
    });
}

async Task comboPartyMode()
{
    await WithServoAndLed(async s =>
    {
        Console.WriteLine("Party mode start.");
        for (int i = 0; i < 16; i++)
        {
            s.WriteAngle(random.Next(0, 181));
            gpioController.Write(ledPin, PinValue.High);
            await Task.Delay(90);
            gpioController.Write(ledPin, PinValue.Low);
            await Task.Delay(90);
        }
        Console.WriteLine("Party mode complete.");
    });
}

async Task comboCountdown()
{
    await WithServoAndLed(async s =>
    {
        Console.WriteLine("Countdown start.");
        int[] positions = { 180, 120, 60, 0 };
        foreach (int angle in positions)
        {
            s.WriteAngle(angle);
            gpioController.Write(ledPin, PinValue.High);
            await Task.Delay(350);
            gpioController.Write(ledPin, PinValue.Low);
            await Task.Delay(220);
        }
        for (int i = 0; i < 5; i++)
        {
            gpioController.Write(ledPin, PinValue.High);
            await Task.Delay(80);
            gpioController.Write(ledPin, PinValue.Low);
            await Task.Delay(80);
        }
        s.WriteAngle(90);
        Console.WriteLine("Countdown complete.");
    });
}

async Task comboAlarm()
{
    await WithServoAndLed(async s =>
    {
        Console.WriteLine("Alarm combo start.");
        for (int i = 0; i < 8; i++)
        {
            s.WriteAngle(i % 2 == 0 ? 20 : 160);
            gpioController.Write(ledPin, PinValue.High);
            await Task.Delay(160);
            gpioController.Write(ledPin, PinValue.Low);
            await Task.Delay(140);
        }
        s.WriteAngle(90);
        Console.WriteLine("Alarm combo complete.");
    });
}

async Task comboCelebration()
{
    await WithServoAndLed(async s =>
    {
        Console.WriteLine("Celebration combo start.");
        for (int i = 0; i < 10; i++)
        {
            s.WriteAngle(random.Next(0, 181));
            gpioController.Write(ledPin, PinValue.High);
            await Task.Delay(60 + (i % 3) * 20);
            gpioController.Write(ledPin, PinValue.Low);
            await Task.Delay(60 + ((i + 1) % 3) * 20);
        }
        Console.WriteLine("Celebration combo complete.");
    });
}

async Task comboPulseSync()
{
    await WithServoAndLed(async s =>
    {
        Console.WriteLine("Pulse sync combo start.");
        int[] tempo = { 90, 60, 120, 60 };
        for (int cycle = 0; cycle < 4; cycle++)
        {
            s.WriteAngle(80);
            gpioController.Write(ledPin, PinValue.High);
            await Task.Delay(tempo[cycle % tempo.Length]);
            s.WriteAngle(100);
            await Task.Delay(tempo[cycle % tempo.Length]);
            gpioController.Write(ledPin, PinValue.Low);
            await Task.Delay(tempo[cycle % tempo.Length] + 80);
        }
        s.WriteAngle(90);
        Console.WriteLine("Pulse sync combo complete.");
    });
}

async Task comboGuardSweep()
{
    await WithServoAndLed(async s =>
    {
        Console.WriteLine("Guard sweep combo start.");
        int[] checkpoints = { 20, 60, 120, 160, 120, 60, 20 };
        foreach (int angle in checkpoints)
        {
            s.WriteAngle(angle);
            gpioController.Write(ledPin, PinValue.High);
            await Task.Delay(260);
            gpioController.Write(ledPin, PinValue.Low);
            await Task.Delay(180);
        }
        s.WriteAngle(90);
        Console.WriteLine("Guard sweep combo complete.");
    });
}

async Task comboRandomShow()
{
    await WithServoAndLed(async s =>
    {
        Console.WriteLine("Random show combo start.");
        for (int i = 0; i < 12; i++)
        {
            int angle = random.Next(0, 181);
            s.WriteAngle(angle);
            bool ledOn = random.NextDouble() > 0.5;
            gpioController.Write(ledPin, ledOn ? PinValue.High : PinValue.Low);
            await Task.Delay(random.Next(120, 360));
        }
        gpioController.Write(ledPin, PinValue.Low);
        s.WriteAngle(90);
        Console.WriteLine("Random show combo complete.");
    });
}

// Rotate endpoint
app.MapGet("/rotate/{angle:int}", async (int angle) =>
{
    if (angle < 0 || angle > 180)
        return Results.Json(new { ok = false, error = "Angle must be between 0 and 180." }, statusCode: 400);
    await rotateServo(angle);
    return Results.Json(new { ok = true, angle = lastServoAngle });
});

app.MapGet("/wave", async () =>
{
    await waveServo();
    return Results.Json(new { ok = true, routine = "wave" });
});

app.MapGet("/servo/smooth-sweep", async () =>
{
    await servoSmoothSweep();
    return Results.Json(new { ok = true, routine = "smooth-sweep" });
});

app.MapGet("/servo/random-dance", async () =>
{
    await servoRandomDance();
    return Results.Content(
        "<html><body><h1>Random dance complete</h1></body></html>",
        "text/html"
    );
});

app.MapGet("/servo/jitter", async () =>
{
    await servoJitter();
    return Results.Content(
        "<html><body><h1>Jitter routine complete</h1></body></html>",
        "text/html"
    );
});

app.MapGet("/servo/pose-sequence", async () =>
{
    await servoPoseSequence();
    return Results.Content(
        "<html><body><h1>Pose sequence complete</h1></body></html>",
        "text/html"
    );
});

app.MapGet("/servo/micro-sweep", async () =>
{
    await servoMicroSweep();
    return Results.Content(
        "<html><body><h1>Micro sweep complete</h1></body></html>",
        "text/html"
    );
});

app.MapGet("/servo/slow-pan", async () =>
{
    await servoSlowPan();
    return Results.Content(
        "<html><body><h1>Slow pan complete</h1></body></html>",
        "text/html"
    );
});

app.MapGet("/servo/double-wave", async () =>
{
    await servoDoubleWave();
    return Results.Content(
        "<html><body><h1>Double wave complete</h1></body></html>",
        "text/html"
    );
});

app.MapGet("/servo/focus-sweep", async () =>
{
    await servoFocusSweep();
    return Results.Content(
        "<html><body><h1>Focus sweep complete</h1></body></html>",
        "text/html"
    );
});

app.MapGet("/servo/sine-ride", async () =>
{
    await servoSineRide();
    return Results.Content(
        "<html><body><h1>Sine ride complete</h1></body></html>",
        "text/html"
    );
});

app.MapGet("/servo/random-pause", async () =>
{
    await servoRandomPause();
    return Results.Content(
        "<html><body><h1>Random pause routine complete</h1></body></html>",
        "text/html"
    );
});

app.MapGet("/servo/edge-pulse", async () =>
{
    await servoEdgePulse();
    return Results.Content(
        "<html><body><h1>Edge pulse complete</h1></body></html>",
        "text/html"
    );
});

app.MapGet("/servo/drum-roll", async () =>
{
    await servoDrumRoll();
    return Results.Content(
        "<html><body><h1>Drum roll complete</h1></body></html>",
        "text/html"
    );
});

app.MapGet("/servo/spin-cycle", async () =>
{
    await servoSpinCycle();
    return Results.Content(
        "<html><body><h1>Spin cycle complete</h1></body></html>",
        "text/html"
    );
});

app.MapGet("/servo/heartbeat", async () =>
{
    await servoHeartbeatArc();
    return Results.Content(
        "<html><body><h1>Heartbeat arc complete</h1></body></html>",
        "text/html"
    );
});

app.MapGet("/servo/rain-drop", async () =>
{
    await servoRainDrop();
    return Results.Content(
        "<html><body><h1>Raindrop routine complete</h1></body></html>",
        "text/html"
    );
});

app.MapGet("/led/strobe", async () =>
{
    if (ThrottleLedPattern()) return Results.Json(new { ok = false, error = "Pattern throttled" }, statusCode: 429);
    await ledStrobe();
    return Results.Json(new { ok = true, pattern = "strobe" });
});

app.MapGet("/led/pulse", async () =>
{
    await ledPulse();
    return Results.Content(
        "<html><body><h1>LED pulse complete</h1></body></html>",
        "text/html"
    );
});

app.MapGet("/led/sos", async () =>
{
    await ledSos();
    return Results.Content(
        "<html><body><h1>LED SOS complete</h1></body></html>",
        "text/html"
    );
});

app.MapGet("/led/breathing", async () =>
{
    if (ThrottleLedPattern()) return Results.Json(new { ok = false, error = "Pattern throttled" }, statusCode: 429);
    await ledBreathing();
    return Results.Json(new { ok = true, pattern = "breathing" });
});

app.MapGet("/led/blink-burst", async () =>
{
    await ledBlinkBurst();
    return Results.Content(
        "<html><body><h1>LED blink burst complete</h1></body></html>",
        "text/html"
    );
});

app.MapGet("/led/random-sparkle", async () =>
{
    await ledRandomSparkle();
    return Results.Content(
        "<html><body><h1>LED random sparkle complete</h1></body></html>",
        "text/html"
    );
});

app.MapGet("/led/long-flash", async () =>
{
    await ledLongFlash();
    return Results.Content(
        "<html><body><h1>LED long flash complete</h1></body></html>",
        "text/html"
    );
});

app.MapGet("/led/twinkle", async () =>
{
    await ledTwinkle();
    return Results.Content(
        "<html><body><h1>LED twinkle complete</h1></body></html>",
        "text/html"
    );
});

app.MapGet("/combo/party", async () =>
{
    await comboPartyMode();
    return Results.Content(
        "<html><body><h1>Party mode complete</h1></body></html>",
        "text/html"
    );
});

app.MapGet("/combo/countdown", async () =>
{
    await comboCountdown();
    return Results.Content(
        "<html><body><h1>Countdown complete</h1></body></html>",
        "text/html"
    );
});

app.MapGet("/combo/alarm", async () =>
{
    await comboAlarm();
    return Results.Content(
        "<html><body><h1>Alarm combo complete</h1></body></html>",
        "text/html"
    );
});

app.MapGet("/combo/celebration", async () =>
{
    await comboCelebration();
    return Results.Content(
        "<html><body><h1>Celebration combo complete</h1></body></html>",
        "text/html"
    );
});

app.MapGet("/combo/pulse-sync", async () =>
{
    await comboPulseSync();
    return Results.Content(
        "<html><body><h1>Pulse sync combo complete</h1></body></html>",
        "text/html"
    );
});

app.MapGet("/combo/guard-sweep", async () =>
{
    await comboGuardSweep();
    return Results.Content(
        "<html><body><h1>Guard sweep combo complete</h1></body></html>",
        "text/html"
    );
});

app.MapGet("/combo/random-show", async () =>
{
    await comboRandomShow();
    return Results.Content(
        "<html><body><h1>Random show combo complete</h1></body></html>",
        "text/html"
    );
});

// Echo numbers (GET)
app.MapGet("/numbers", (int one, int two, int three) =>
{
    var output = $"Received numbers: {one}, {two}, {three}";
    Console.WriteLine(output);

    return Results.Content($@"<html>
  <body>
    <h1>Numbers Received</h1>
    <p>{one}, {two}, {three}</p>
  </body>
</html>", "text/html");
});

// LED control
app.MapGet("/gpio/on", () =>
{
    gpioController.Write(ledPin, PinValue.High);
    ledOn = true;
    Console.WriteLine("LED turned ON");
    return Results.Json(new { ok = true, led = "on" });
});

app.MapGet("/gpio/off", () =>
{
    gpioController.Write(ledPin, PinValue.Low);
    ledOn = false;
    Console.WriteLine("LED turned OFF");
    return Results.Json(new { ok = true, led = "off" });
});

// Health & state endpoints
app.MapGet("/health", () => Results.Json(new { status = "ok" }));
app.MapGet("/state", () => Results.Json(new { servoAngle = lastServoAngle, ledOn }));

// Echo numbers (POST JSON: { "one": 1, "two": 2, "three": 3 })
app.MapPost("/numbers", (NumbersPayload payload) =>
{
    var output = $"Received numbers: {payload.One}, {payload.Two}, {payload.Three}";
    Console.WriteLine(output);
    return Results.Json(new { message = output, numbers = payload });
});

app.Run();

public record NumbersPayload(int One, int Two, int Three);
LEGACY MONOLITH END */

// Refactored minimal composition root below
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using ConsoleApp1.Api;
using ConsoleApp1.Hardware;
using ConsoleApp1.Control;
using ConsoleApp1; // HtmlLoader

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://0.0.0.0:5000");

// Configure JSON serialization to use camelCase property names (matches JavaScript conventions)
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});

builder.Services.AddSingleton<HardwareLocks>();
builder.Services.AddSingleton<ServoController>();
builder.Services.AddSingleton<LedController>();
builder.Services.AddSingleton<Mpu6050>();
builder.Services.AddSingleton<PidController>();

var app = builder.Build();
app.UseStaticFiles();

app.MapGet("/", () =>
{
    var html = HtmlLoader.LoadHtml("index.html");
    return Results.Content(html, "text/html");
});

app.MapGet("/gyro/visualizer", () =>
{
    var html = HtmlLoader.LoadHtml("gyro-visualizer.html");
    return Results.Content(html, "text/html");
});

app.MapServoEndpoints();
app.MapLedEndpoints();
app.MapComboEndpoints();
app.MapGyroEndpoints();
app.MapPidEndpoints();

app.MapGet("/health", (ServoController servo, LedController led, Mpu6050 imu) => Results.Json(new { status = "ok", servoReady = true, ledReady = true, imuReady = true }));
app.MapGet("/state", (ServoController servo, LedController led, Mpu6050 imu) => Results.Json(new { servoAngle = servo.CurrentAngle, ledOn = led.IsOn, imuRaw = imu.LastRaw, imuAngles = imu.LastAngles }));

app.MapGet("/numbers", (int one, int two, int three) => Results.Json(new { one, two, three }));
app.MapPost("/numbers", (NumbersPayload payload) => Results.Json(new { payload }));

app.Run();

public record NumbersPayload(int One, int Two, int Three);
