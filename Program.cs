
using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Device.Gpio;
using System.Device.Pwm;
using ConsoleApp1;

var builder = WebApplication.CreateBuilder(args);

// Configure the server to listen on all network interfaces
builder.WebHost.UseUrls("http://0.0.0.0:5000");

var app = builder.Build();
const int pin = 17;

// Use GPIO18 for servo control (hardware PWM pin)
const int servoPin = 18;

app.MapGet("/", () =>
{
    var htmlContent = HtmlLoader.LoadHtml("index.html");
    return Results.Content(htmlContent, "text/html");
});

// Initialize GPIO pin for output
var gpioController = new GpioController();
gpioController.OpenPin(pin, PinMode.Output);
gpioController.Write(pin, PinValue.Low); // Ensure LED is off initially
Console.WriteLine($"GPIO pin {pin} initialized for output.");

// Initialize servo pin for output
gpioController.OpenPin(servoPin, PinMode.Output);
gpioController.Write(servoPin, PinValue.Low);
Console.WriteLine($"GPIO pin {servoPin} initialized for servo control.");

async Task rotateServo(int angle, GpioController controller)
{
  // Standard hobby servos use a 50Hz PWM signal (20ms period = 20000 microseconds).
  const int periodMicroseconds = 20000;
  
  // Calculate pulse width in microseconds (500-2500 µs for 0-180 degrees)
  double minPulseMicroseconds = 500;
  double maxPulseMicroseconds = 2500;
  double pulseMicroseconds = minPulseMicroseconds + (angle / 180.0) * (maxPulseMicroseconds - minPulseMicroseconds);
  
  int pulseWidthMicros = (int)pulseMicroseconds;
  int offTimeMicros = periodMicroseconds - pulseWidthMicros;
  
  // Send PWM signal for about 1 second to ensure servo reaches position
  var stopwatch = System.Diagnostics.Stopwatch.StartNew();
  while (stopwatch.ElapsedMilliseconds < 1000)
  {
    controller.Write(servoPin, PinValue.High);
    await Task.Delay(TimeSpan.FromMicroseconds(pulseWidthMicros));
    controller.Write(servoPin, PinValue.Low);
    await Task.Delay(TimeSpan.FromMicroseconds(offTimeMicros));
  }
}

app.MapGet("/rotate/{angle:int}", async (int angle) =>
{
  if (angle < 0 || angle > 180)
  {
    return Results.BadRequest("Angle must be between 0 and 180 degrees.");
  }

  await rotateServo(angle, gpioController);
  return Results.Content($"<html><body><h1>Servo rotated to {angle} degrees</h1></body></html>", "text/html");
});

app.MapGet("/numbers", (int one, int two, int three) =>
{
  var output = $"Received numbers: {one}, {two}, {three}";
  // Print to the server console (output screen)
  Console.WriteLine(output);

  // Return a simple HTML response so the caller also sees them
  return Results.Content($@"<html>
  <body>
    <h1>Numbers Received</h1>
    <p>{one}, {two}, {three}</p>
  </body>
</html>", "text/html");
});

app.MapGet("/gpio/on", () =>
{
  gpioController.Write(pin, PinValue.High);
  Console.WriteLine("LED turned ON");
  return Results.Content("<html><body><h1>LED is ON</h1></body></html>", "text/html");
});

app.MapGet("/gpio/off", () =>
{
  gpioController.Write(pin, PinValue.Low);
  Console.WriteLine("LED turned OFF");
  return Results.Content("<html><body><h1>LED is OFF</h1></body></html>", "text/html");
});



// Accept JSON POST like: { "one": 1, "two": 2, "three": 3 }
app.MapPost("/numbers", (NumbersPayload payload) =>
{
  var output = $"Received numbers: {payload.One}, {payload.Two}, {payload.Three}";
  Console.WriteLine(output);

  // Return a JSON response with the values echoed back
  return Results.Json(new { message = output, numbers = payload });
});

app.Run();

public record NumbersPayload(int One, int Two, int Three);