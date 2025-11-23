using ConsoleApp1.Hardware;

namespace ConsoleApp1.Api;

/// <summary>Maps LED related endpoints.</summary>
public static class LedEndpoints
{
    public static void MapLedEndpoints(this WebApplication app)
    {
        app.MapGet("/gpio/on", (LedController led) => { led.TurnOn(); return Results.Json(new { ok=true, led="on" }); });
        app.MapGet("/gpio/off", (LedController led) => { led.TurnOff(); return Results.Json(new { ok=true, led="off" }); });

        app.MapGet("/led/strobe", async (LedController led) =>
        {
            if (led.Throttled()) return Results.Json(new { ok=false, error="Pattern throttled" }, statusCode:429);
            await led.StrobeAsync(); return Results.Json(new { ok=true, pattern="strobe" });
        });
        app.MapGet("/led/pulse", async (LedController led) => { await led.PulseAsync(); return Results.Json(new { ok=true, pattern="pulse" }); });
        app.MapGet("/led/sos", async (LedController led) => { await led.SosAsync(); return Results.Json(new { ok=true, pattern="sos" }); });
        app.MapGet("/led/breathing", async (LedController led) => { if (led.Throttled()) return Results.Json(new { ok=false, error="Pattern throttled" }, statusCode:429); await led.BreathingAsync(); return Results.Json(new { ok=true, pattern="breathing" }); });
        app.MapGet("/led/blink-burst", async (LedController led) => { await led.BlinkBurstAsync(); return Results.Json(new { ok=true, pattern="blink-burst" }); });
        app.MapGet("/led/random-sparkle", async (LedController led) => { await led.RandomSparkleAsync(); return Results.Json(new { ok=true, pattern="random-sparkle" }); });
        app.MapGet("/led/long-flash", async (LedController led) => { await led.LongFlashAsync(); return Results.Json(new { ok=true, pattern="long-flash" }); });
        app.MapGet("/led/twinkle", async (LedController led) => { await led.TwinkleAsync(); return Results.Json(new { ok=true, pattern="twinkle" }); });
    }
}
