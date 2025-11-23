using ConsoleApp1.Hardware;
using ConsoleApp1.Models;
using Microsoft.Extensions.Logging;

namespace ConsoleApp1.Api;

/// <summary>Gyro (MPU-6050) data & fun motion routines.</summary>
public static class GyroEndpoints
{
    public static void MapGyroEndpoints(this WebApplication app)
    {
        // Sensor status endpoint
        app.MapGet("/gyro/status", (Mpu6050 imu) =>
        {
            return Results.Json(new
            {
                available = imu.IsAvailable,
                lastError = imu.GetLastErrorMessage(),
                lastUpdate = imu.LastUpdate
            });
        });

        // Basic data endpoints
        app.MapGet("/gyro/raw", async (Mpu6050 imu, ILogger<Mpu6050> logger) =>
        {
            try
            {
                var raw = await imu.ReadRawAsync();
                return Results.Json(raw);
            }
            catch (SensorUnavailableException ex)
            {
                logger.LogWarning("Sensor unavailable: {Message}", ex.Message);
                return Results.Problem(
                    title: "MPU-6050 sensor unavailable",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status503ServiceUnavailable);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error accessing MPU-6050");
                return Results.Problem(
                    title: "Unexpected MPU-6050 error",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        });

        app.MapGet("/gyro/angles", async (Mpu6050 imu, ILogger<Mpu6050> logger) =>
        {
            try
            {
                var angles = await imu.UpdateAnglesAsync();
                return Results.Json(angles);
            }
            catch (SensorUnavailableException ex)
            {
                logger.LogWarning("Sensor unavailable: {Message}", ex.Message);
                return Results.Problem(
                    title: "MPU-6050 sensor unavailable",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status503ServiceUnavailable);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error accessing MPU-6050");
                return Results.Problem(
                    title: "Unexpected MPU-6050 error",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        });

        app.MapGet("/gyro/magnitude", async (Mpu6050 imu, ILogger<Mpu6050> logger) =>
        {
            try
            {
                await imu.ReadRawAsync();
                return Results.Json(new { magnitude = imu.GetGyroMagnitude() });
            }
            catch (SensorUnavailableException ex)
            {
                logger.LogWarning("Sensor unavailable: {Message}", ex.Message);
                return Results.Problem(
                    title: "MPU-6050 sensor unavailable",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status503ServiceUnavailable);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error accessing MPU-6050");
                return Results.Problem(
                    title: "Unexpected MPU-6050 error",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        });

        app.MapGet("/gyro/freefall", async (Mpu6050 imu, ILogger<Mpu6050> logger) =>
        {
            try
            {
                await imu.ReadRawAsync();
                return Results.Json(new { freefall = imu.IsFreeFall() });
            }
            catch (SensorUnavailableException ex)
            {
                logger.LogWarning("Sensor unavailable: {Message}", ex.Message);
                return Results.Problem(
                    title: "MPU-6050 sensor unavailable",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status503ServiceUnavailable);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error accessing MPU-6050");
                return Results.Problem(
                    title: "Unexpected MPU-6050 error",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        });

        app.MapGet("/gyro/stabilityCheck", async (Mpu6050 imu, ILogger<Mpu6050> logger) =>
        {
            try
            {
                await imu.ReadRawAsync();
                return Results.Json(new { stable = imu.IsStable() });
            }
            catch (SensorUnavailableException ex)
            {
                logger.LogWarning("Sensor unavailable: {Message}", ex.Message);
                return Results.Problem(
                    title: "MPU-6050 sensor unavailable",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status503ServiceUnavailable);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error accessing MPU-6050");
                return Results.Problem(
                    title: "Unexpected MPU-6050 error",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        });

        // Update angles explicitly (forces read)
        app.MapGet("/gyro/update", async (Mpu6050 imu, ILogger<Mpu6050> logger) =>
        {
            try
            {
                return Results.Json(await imu.UpdateAnglesAsync());
            }
            catch (SensorUnavailableException ex)
            {
                logger.LogWarning("Sensor unavailable: {Message}", ex.Message);
                return Results.Problem(
                    title: "MPU-6050 sensor unavailable",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status503ServiceUnavailable);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error accessing MPU-6050");
                return Results.Problem(
                    title: "Unexpected MPU-6050 error",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        });

        // 30+ fun routines using servo & LED based on orientation / motion.
        // All wrapped with error handling to prevent crashes
        
        app.MapGet("/gyro/fun/tilt-servo-map", async (Mpu6050 imu, ServoController servo, ILogger<Mpu6050> logger) =>
        {
            try
            {
                var angles = await imu.UpdateAnglesAsync();
                int target = (int)(90 + angles.Roll);
                await servo.RotateAsync(target);
                return Results.Json(new GyroRoutineResult("tilt-servo-map", true, "Mapped roll to servo angle", new { target }));
            }
            catch (SensorUnavailableException ex) { return SensorError(logger, ex); }
            catch (Exception ex) { return UnexpectedError(logger, ex); }
        });

        app.MapGet("/gyro/fun/shake-led-strobe", async (Mpu6050 imu, LedController led, ILogger<Mpu6050> logger) =>
        {
            try
            {
                await imu.ReadRawAsync();
                if (imu.GetGyroMagnitude() > 50)
                {
                    await led.StrobeAsync();
                    return Results.Json(new GyroRoutineResult("shake-led-strobe", true, "Shake detected -> strobe"));
                }
                return Results.Json(new GyroRoutineResult("shake-led-strobe", false, "No shake"));
            }
            catch (SensorUnavailableException ex) { return SensorError(logger, ex); }
            catch (Exception ex) { return UnexpectedError(logger, ex); }
        });

        app.MapGet("/gyro/fun/level-guard", async (Mpu6050 imu, LedController led, ILogger<Mpu6050> logger) =>
        {
            try
            {
                var angles = await imu.UpdateAnglesAsync();
                bool level = Math.Abs(angles.Pitch) < 5 && Math.Abs(angles.Roll) < 5;
                if (level) led.TurnOn(); else led.TurnOff();
                return Results.Json(new GyroRoutineResult("level-guard", true, level?"Level -> LED ON":"Not level -> LED OFF", new { level }));
            }
            catch (SensorUnavailableException ex) { return SensorError(logger, ex); }
            catch (Exception ex) { return UnexpectedError(logger, ex); }
        });

        app.MapGet("/gyro/fun/freefall-alarm", async (Mpu6050 imu, LedController led, ILogger<Mpu6050> logger) =>
        {
            try
            {
                await imu.ReadRawAsync();
                if (imu.IsFreeFall()) { await led.BlinkBurstAsync(); return Results.Json(new GyroRoutineResult("freefall-alarm", true, "Freefall pattern triggered")); }
                return Results.Json(new GyroRoutineResult("freefall-alarm", false, "No freefall"));
            }
            catch (SensorUnavailableException ex) { return SensorError(logger, ex); }
            catch (Exception ex) { return UnexpectedError(logger, ex); }
        });

        app.MapGet("/gyro/fun/tilt-sweep", async (Mpu6050 imu, ServoController servo, ILogger<Mpu6050> logger) => 
        { 
            try { await servo.SmoothSweepAsync(); return Results.Json(new GyroRoutineResult("tilt-sweep", true, "Performed smooth sweep regardless of tilt")); }
            catch (SensorUnavailableException ex) { return SensorError(logger, ex); }
            catch (Exception ex) { return UnexpectedError(logger, ex); }
        });

        app.MapGet("/gyro/fun/orientation-wave", async (ServoController servo, ILogger<Mpu6050> logger) => 
        { 
            try { await servo.WaveAsync(); return Results.Json(new GyroRoutineResult("orientation-wave", true, "Wave sequence")); }
            catch (Exception ex) { return UnexpectedError(logger, ex); }
        });
        app.MapGet("/gyro/fun/shake-record-start", (HttpContext ctx) => Results.Json(new GyroRoutineResult("shake-record-start", true, "Recording start stub")));
        app.MapGet("/gyro/fun/shake-record-stop", (HttpContext ctx) => Results.Json(new GyroRoutineResult("shake-record-stop", true, "Recording stop stub")));
        app.MapGet("/gyro/fun/shake-replay", async (ServoController servo, ILogger<Mpu6050> logger) => 
        { 
            try { await servo.JitterAsync(); return Results.Json(new GyroRoutineResult("shake-replay", true, "Replayed jitter pattern")); }
            catch (Exception ex) { return UnexpectedError(logger, ex); }
        });

        app.MapGet("/gyro/fun/gyro-magnitude-blink", async (Mpu6050 imu, LedController led, ILogger<Mpu6050> logger) =>
        {
            try
            {
                await imu.ReadRawAsync();
                int flashes = (int)Math.Clamp(imu.GetGyroMagnitude()/10,1,10);
                await led.PatternFlashAsync(flashes,80,80);
                return Results.Json(new GyroRoutineResult("gyro-magnitude-blink", true, "Blink scaled to magnitude", new { flashes }));
            }
            catch (SensorUnavailableException ex) { return SensorError(logger, ex); }
            catch (Exception ex) { return UnexpectedError(logger, ex); }
        });

        app.MapGet("/gyro/fun/pitch-roll-pan", async (Mpu6050 imu, ServoController servo, ILogger<Mpu6050> logger) => 
        { 
            try
            {
                var angles = await imu.UpdateAnglesAsync();
                int target=(int)(90 + angles.Pitch);
                await servo.RotateAsync(target);
                return Results.Json(new GyroRoutineResult("pitch-roll-pan", true, "Pitch mapped to servo", new { target }));
            }
            catch (SensorUnavailableException ex) { return SensorError(logger, ex); }
            catch (Exception ex) { return UnexpectedError(logger, ex); }
        });

        app.MapGet("/gyro/fun/yaw-spin-cycle", async (ServoController servo, ILogger<Mpu6050> logger) => 
        { 
            try { await servo.SpinCycleAsync(); return Results.Json(new GyroRoutineResult("yaw-spin-cycle", true, "Spin cycle")); }
            catch (Exception ex) { return UnexpectedError(logger, ex); }
        });

        app.MapGet("/gyro/fun/stable-hold-game", async (Mpu6050 imu, LedController led, ILogger<Mpu6050> logger) => 
        { 
            try
            {
                await imu.ReadRawAsync();
                bool stable=imu.IsStable();
                if(stable) led.TurnOn(); else led.TurnOff();
                return Results.Json(new GyroRoutineResult("stable-hold-game", true, stable?"Stable hold success":"Keep steady", new { stable }));
            }
            catch (SensorUnavailableException ex) { return SensorError(logger, ex); }
            catch (Exception ex) { return UnexpectedError(logger, ex); }
        });

        app.MapGet("/gyro/fun/dont-shake-game", async (Mpu6050 imu, LedController led, ILogger<Mpu6050> logger) => 
        { 
            try
            {
                await imu.ReadRawAsync();
                bool shake=imu.GetGyroMagnitude()>40;
                if(shake) { await led.StrobeAsync(); }
                return Results.Json(new GyroRoutineResult("dont-shake-game", true, shake?"You shook it!":"All calm"));
            }
            catch (SensorUnavailableException ex) { return SensorError(logger, ex); }
            catch (Exception ex) { return UnexpectedError(logger, ex); }
        });

        app.MapGet("/gyro/fun/balance-challenge", async (Mpu6050 imu, LedController led, ILogger<Mpu6050> logger) => 
        { 
            try
            {
                var angles = await imu.UpdateAnglesAsync();
                bool balanced=Math.Abs(angles.Pitch)<3 && Math.Abs(angles.Roll)<3;
                if(balanced) led.TurnOn(); else led.TurnOff();
                return Results.Json(new GyroRoutineResult("balance-challenge", true, balanced?"Balanced":"Not balanced"));
            }
            catch (SensorUnavailableException ex) { return SensorError(logger, ex); }
            catch (Exception ex) { return UnexpectedError(logger, ex); }
        });

        app.MapGet("/gyro/fun/noise-react", async (Mpu6050 imu, LedController led, ILogger<Mpu6050> logger) => 
        { 
            try
            {
                await imu.ReadRawAsync();
                double mag=imu.GetGyroMagnitude();
                if(mag>30) await led.RandomSparkleAsync();
                return Results.Json(new GyroRoutineResult("noise-react", true, mag>30?"Sparkle":"Quiet", new { mag }));
            }
            catch (SensorUnavailableException ex) { return SensorError(logger, ex); }
            catch (Exception ex) { return UnexpectedError(logger, ex); }
        });

        app.MapGet("/gyro/fun/motion-random-combo", async (Mpu6050 imu, ServoController servo, LedController led, HardwareLocks locks, ILogger<Mpu6050> logger) =>
        {
            try
            {
                await locks.ComboLock.WaitAsync();
                try
                {
                    await imu.ReadRawAsync();
                    double mag=imu.GetGyroMagnitude();
                    int loops=(int)Math.Clamp(mag/15,1,8); var rand=new Random();
                    for(int i=0;i<loops;i++){ servo.WriteAngle(rand.Next(0,181)); led.TurnOn(); await Task.Delay(100); led.TurnOff(); await Task.Delay(100);}    
                    return Results.Json(new GyroRoutineResult("motion-random-combo", true, "Combo based on motion loops", new { loops }));
                }
                finally { locks.ComboLock.Release(); }
            }
            catch (SensorUnavailableException ex) { return SensorError(logger, ex); }
            catch (Exception ex) { return UnexpectedError(logger, ex); }
        });

        app.MapGet("/gyro/fun/pitch-pulse-led", async (Mpu6050 imu, LedController led, ILogger<Mpu6050> logger) => 
        { 
            try
            {
                var angles = await imu.UpdateAnglesAsync();
                int flashes=(int)Math.Clamp(Math.Abs(angles.Pitch)/10,1,10);
                await led.PatternFlashAsync(flashes,120,120);
                return Results.Json(new GyroRoutineResult("pitch-pulse-led", true, "Pitch pulsed LED", new { flashes }));
            }
            catch (SensorUnavailableException ex) { return SensorError(logger, ex); }
            catch (Exception ex) { return UnexpectedError(logger, ex); }
        });

        app.MapGet("/gyro/fun/roll-focus-servo", async (Mpu6050 imu, ServoController servo, ILogger<Mpu6050> logger) => 
        { 
            try { await servo.FocusSweepAsync(); return Results.Json(new GyroRoutineResult("roll-focus-servo", true, "Focus sweep run")); }
            catch (SensorUnavailableException ex) { return SensorError(logger, ex); }
            catch (Exception ex) { return UnexpectedError(logger, ex); }
        });

        app.MapGet("/gyro/fun/angle-double-wave", async (ServoController servo, ILogger<Mpu6050> logger) => 
        { 
            try { await servo.DoubleWaveAsync(); return Results.Json(new GyroRoutineResult("angle-double-wave", true, "Double wave executed")); }
            catch (Exception ex) { return UnexpectedError(logger, ex); }
        });

        app.MapGet("/gyro/fun/impact-flash", async (Mpu6050 imu, LedController led, ILogger<Mpu6050> logger) => 
        { 
            try
            {
                var raw = await imu.ReadRawAsync();
                if(Math.Abs(raw.Az - 1.0) > 0.4) await led.LongFlashAsync();
                return Results.Json(new GyroRoutineResult("impact-flash", true, "Impact check done"));
            }
            catch (SensorUnavailableException ex) { return SensorError(logger, ex); }
            catch (Exception ex) { return UnexpectedError(logger, ex); }
        });

        app.MapGet("/gyro/fun/drift-check", async (Mpu6050 imu, ILogger<Mpu6050> logger) => 
        {
            try
            {
                await imu.ReadRawAsync();
                return Results.Json(new GyroRoutineResult("drift-check", true, "Gyro magnitude", new { mag=imu.GetGyroMagnitude() }));
            }
            catch (SensorUnavailableException ex) { return SensorError(logger, ex); }
            catch (Exception ex) { return UnexpectedError(logger, ex); }
        });

        app.MapGet("/gyro/fun/rest-center", async (ServoController servo, ILogger<Mpu6050> logger) => 
        { 
            try { await servo.RotateAsync(90); return Results.Json(new GyroRoutineResult("rest-center", true, "Centered servo")); }
            catch (Exception ex) { return UnexpectedError(logger, ex); }
        });

        app.MapGet("/gyro/fun/tilt-micro-adjust", async (Mpu6050 imu, ServoController servo, ILogger<Mpu6050> logger) => 
        { 
            try
            {
                var angles = await imu.UpdateAnglesAsync();
                int delta=(int)(angles.Roll/5);
                await servo.RotateAsync(Math.Clamp(servo.CurrentAngle+delta,0,180));
                return Results.Json(new GyroRoutineResult("tilt-micro-adjust", true, "Adjusted servo", new { newAngle=servo.CurrentAngle }));
            }
            catch (SensorUnavailableException ex) { return SensorError(logger, ex); }
            catch (Exception ex) { return UnexpectedError(logger, ex); }
        });

        app.MapGet("/gyro/fun/rapid-twist", async (ServoController servo, ILogger<Mpu6050> logger) => 
        { 
            try { await servo.EdgePulseAsync(); return Results.Json(new GyroRoutineResult("rapid-twist", true, "Edge pulse done")); }
            catch (Exception ex) { return UnexpectedError(logger, ex); }
        });
        app.MapGet("/gyro/fun/slow-pan-gyro", async (ServoController servo, ILogger<Mpu6050> logger) => 
        { 
            try { await servo.SlowPanAsync(); return Results.Json(new GyroRoutineResult("slow-pan-gyro", true, "Slow pan executed")); }
            catch (Exception ex) { return UnexpectedError(logger, ex); }
        });
        app.MapGet("/gyro/fun/breathing-sync", async (LedController led, ILogger<Mpu6050> logger) => 
        { 
            try { await led.BreathingAsync(); return Results.Json(new GyroRoutineResult("breathing-sync", true, "Breathing LED")); }
            catch (Exception ex) { return UnexpectedError(logger, ex); }
        });
        app.MapGet("/gyro/fun/heartbeat-sync", async (ServoController servo, ILogger<Mpu6050> logger) => 
        { 
            try { await servo.HeartbeatArcAsync(); return Results.Json(new GyroRoutineResult("heartbeat-sync", true, "Heartbeat arc")); }
            catch (Exception ex) { return UnexpectedError(logger, ex); }
        });
        app.MapGet("/gyro/fun/edge-pulse-gyro", async (ServoController servo, ILogger<Mpu6050> logger) => 
        { 
            try { await servo.EdgePulseAsync(); return Results.Json(new GyroRoutineResult("edge-pulse-gyro", true, "Edge pulse")); }
            catch (Exception ex) { return UnexpectedError(logger, ex); }
        });
        app.MapGet("/gyro/fun/rain-drop-gyro", async (ServoController servo, ILogger<Mpu6050> logger) => 
        { 
            try { await servo.RainDropAsync(); return Results.Json(new GyroRoutineResult("rain-drop-gyro", true, "Rain drop")); }
            catch (Exception ex) { return UnexpectedError(logger, ex); }
        });
        app.MapGet("/gyro/fun/sine-ride-gyro", async (ServoController servo, ILogger<Mpu6050> logger) => 
        { 
            try { await servo.SineRideAsync(); return Results.Json(new GyroRoutineResult("sine-ride-gyro", true, "Sine ride")); }
            catch (Exception ex) { return UnexpectedError(logger, ex); }
        });
    }

    // Helper methods for consistent error responses
    private static IResult SensorError(ILogger logger, SensorUnavailableException ex)
    {
        logger.LogWarning("Sensor unavailable: {Message}", ex.Message);
        return Results.Problem(
            title: "MPU-6050 sensor unavailable",
            detail: ex.Message,
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }

    private static IResult UnexpectedError(ILogger logger, Exception ex)
    {
        logger.LogError(ex, "Unexpected error in gyro endpoint");
        return Results.Problem(
            title: "Unexpected MPU-6050 error",
            detail: ex.Message,
            statusCode: StatusCodes.Status500InternalServerError);
    }
}
