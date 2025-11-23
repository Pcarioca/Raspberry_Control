using ConsoleApp1.Hardware;

namespace ConsoleApp1.Api;

/// <summary>Maps servo-related endpoints.</summary>
public static class ServoEndpoints
{
    public static void MapServoEndpoints(this WebApplication app)
    {
        app.MapGet("/rotate/{angle:int}", async (ServoController servo, int angle) =>
        {
            if (angle < 0 || angle > 180) return Results.Json(new { ok=false, error="Angle must be 0-180" }, statusCode:400);
            await servo.RotateAsync(angle);
            return Results.Json(new { ok=true, angle = servo.CurrentAngle });
        });

        app.MapGet("/wave", async (ServoController servo) => { await servo.WaveAsync(); return Results.Json(new { ok=true, routine="wave" }); });
        app.MapGet("/servo/smooth-sweep", async (ServoController servo) => { await servo.SmoothSweepAsync(); return Results.Json(new { ok=true, routine="smooth-sweep" }); });
        app.MapGet("/servo/random-dance", async (ServoController servo) => { await servo.RandomDanceAsync(); return Results.Json(new { ok=true, routine="random-dance" }); });
        app.MapGet("/servo/jitter", async (ServoController servo) => { await servo.JitterAsync(); return Results.Json(new { ok=true, routine="jitter" }); });
        app.MapGet("/servo/pose-sequence", async (ServoController servo) => { await servo.PoseSequenceAsync(); return Results.Json(new { ok=true, routine="pose-sequence" }); });
        app.MapGet("/servo/micro-sweep", async (ServoController servo) => { await servo.MicroSweepAsync(); return Results.Json(new { ok=true, routine="micro-sweep" }); });
        app.MapGet("/servo/slow-pan", async (ServoController servo) => { await servo.SlowPanAsync(); return Results.Json(new { ok=true, routine="slow-pan" }); });
        app.MapGet("/servo/double-wave", async (ServoController servo) => { await servo.DoubleWaveAsync(); return Results.Json(new { ok=true, routine="double-wave" }); });
        app.MapGet("/servo/focus-sweep", async (ServoController servo) => { await servo.FocusSweepAsync(); return Results.Json(new { ok=true, routine="focus-sweep" }); });
        app.MapGet("/servo/sine-ride", async (ServoController servo) => { await servo.SineRideAsync(); return Results.Json(new { ok=true, routine="sine-ride" }); });
        app.MapGet("/servo/random-pause", async (ServoController servo) => { await servo.RandomPauseAsync(); return Results.Json(new { ok=true, routine="random-pause" }); });
        app.MapGet("/servo/edge-pulse", async (ServoController servo) => { await servo.EdgePulseAsync(); return Results.Json(new { ok=true, routine="edge-pulse" }); });
        app.MapGet("/servo/drum-roll", async (ServoController servo) => { await servo.DrumRollAsync(); return Results.Json(new { ok=true, routine="drum-roll" }); });
        app.MapGet("/servo/spin-cycle", async (ServoController servo) => { await servo.SpinCycleAsync(); return Results.Json(new { ok=true, routine="spin-cycle" }); });
        app.MapGet("/servo/heartbeat", async (ServoController servo) => { await servo.HeartbeatArcAsync(); return Results.Json(new { ok=true, routine="heartbeat" }); });
        app.MapGet("/servo/rain-drop", async (ServoController servo) => { await servo.RainDropAsync(); return Results.Json(new { ok=true, routine="rain-drop" }); });
    }
}
