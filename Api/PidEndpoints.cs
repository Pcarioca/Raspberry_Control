using ConsoleApp1.Control;

namespace ConsoleApp1.Api;

/// <summary>Placeholder PID endpoints (not implemented yet).</summary>
public static class PidEndpoints
{
    public static void MapPidEndpoints(this WebApplication app)
    {
        app.MapGet("/pid/start", (PidController pid) => Results.Json(new { ok=false, message="PID not implemented yet" }));
        app.MapGet("/pid/stop", (PidController pid) => Results.Json(new { ok=false, message="PID not implemented yet" }));
    }
}
