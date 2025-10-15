
using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Device.Gpio;
using System.Device.Pwm;

var builder = WebApplication.CreateBuilder(args);

// Configure the server to listen on all network interfaces
builder.WebHost.UseUrls("http://0.0.0.0:5000");

var app = builder.Build();

// Use GPIO18 for servo control (hardware PWM pin)
const int servoPin = 18;

app.MapGet("/", () =>
    Results.Content(@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Raspberry Pi Control Panel</title>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }
        
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
            display: flex;
            justify-content: center;
            align-items: center;
            padding: 20px;
        }
        
        .container {
            background: white;
            border-radius: 20px;
            box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
            padding: 40px;
            max-width: 600px;
            width: 100%;
        }
        
        h1 {
            text-align: center;
            color: #333;
            margin-bottom: 10px;
            font-size: 2em;
        }
        
        .subtitle {
            text-align: center;
            color: #666;
            margin-bottom: 40px;
            font-size: 0.9em;
        }
        
        .control-section {
            background: #f8f9fa;
            border-radius: 15px;
            padding: 25px;
            margin-bottom: 25px;
            transition: transform 0.2s;
        }
        
        .control-section:hover {
            transform: translateY(-2px);
            box-shadow: 0 5px 15px rgba(0, 0, 0, 0.1);
        }
        
        .section-title {
            font-size: 1.3em;
            color: #667eea;
            margin-bottom: 20px;
            display: flex;
            align-items: center;
            gap: 10px;
        }
        
        .icon {
            width: 30px;
            height: 30px;
            display: inline-block;
        }
        
        .led-controls {
            display: flex;
            gap: 15px;
            justify-content: center;
        }
        
        .btn {
            flex: 1;
            padding: 15px 30px;
            font-size: 1.1em;
            border: none;
            border-radius: 10px;
            cursor: pointer;
            transition: all 0.3s;
            font-weight: 600;
            text-transform: uppercase;
            letter-spacing: 1px;
        }
        
        .btn:hover {
            transform: translateY(-2px);
            box-shadow: 0 5px 15px rgba(0, 0, 0, 0.2);
        }
        
        .btn:active {
            transform: translateY(0);
        }
        
        .btn-on {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
        }
        
        .btn-off {
            background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);
            color: white;
        }
        
        .servo-controls {
            display: flex;
            flex-direction: column;
            gap: 20px;
        }
        
        .slider-container {
            display: flex;
            flex-direction: column;
            gap: 10px;
        }
        
        .slider-label {
            display: flex;
            justify-content: space-between;
            align-items: center;
            color: #555;
            font-weight: 500;
        }
        
        .angle-display {
            font-size: 1.5em;
            color: #667eea;
            font-weight: bold;
        }
        
        input[type='range'] {
            width: 100%;
            height: 8px;
            border-radius: 5px;
            background: #ddd;
            outline: none;
            -webkit-appearance: none;
        }
        
        input[type='range']::-webkit-slider-thumb {
            -webkit-appearance: none;
            appearance: none;
            width: 25px;
            height: 25px;
            border-radius: 50%;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            cursor: pointer;
            box-shadow: 0 2px 5px rgba(0, 0, 0, 0.2);
        }
        
        input[type='range']::-moz-range-thumb {
            width: 25px;
            height: 25px;
            border-radius: 50%;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            cursor: pointer;
            box-shadow: 0 2px 5px rgba(0, 0, 0, 0.2);
            border: none;
        }
        
        .btn-rotate {
            background: linear-gradient(135deg, #84fab0 0%, #8fd3f4 100%);
            color: #333;
        }
        
        .status {
            text-align: center;
            padding: 15px;
            border-radius: 10px;
            margin-top: 10px;
            font-weight: 500;
            display: none;
        }
        
        .status.show {
            display: block;
        }
        
        .status.success {
            background: #d4edda;
            color: #155724;
        }
        
        .status.error {
            background: #f8d7da;
            color: #721c24;
        }
        
        .quick-angles {
            display: grid;
            grid-template-columns: repeat(3, 1fr);
            gap: 10px;
            margin-top: 15px;
        }
        
        .btn-quick {
            padding: 10px;
            background: white;
            border: 2px solid #667eea;
            color: #667eea;
            border-radius: 8px;
            cursor: pointer;
            font-weight: 600;
            transition: all 0.3s;
        }
        
        .btn-quick:hover {
            background: #667eea;
            color: white;
            transform: translateY(-2px);
        }
        
        @media (max-width: 500px) {
            .container {
                padding: 25px;
            }
            
            h1 {
                font-size: 1.5em;
            }
            
            .led-controls {
                flex-direction: column;
            }
        }
    </style>
</head>
<body>
    <div class='container'>
        <h1>🤖 Raspberry Pi Control Panel</h1>
        <div class='subtitle'>Control your devices with ease</div>
        
        <!-- LED Control Section -->
        <div class='control-section'>
            <div class='section-title'>
                <span class='icon'>💡</span>
                LED Control (GPIO 17)
            </div>
            <div class='led-controls'>
                <button class='btn btn-on' onclick='controlLED(true)'>Turn ON</button>
                <button class='btn btn-off' onclick='controlLED(false)'>Turn OFF</button>
            </div>
            <div id='led-status' class='status'></div>
        </div>
        
        <!-- Servo Control Section -->
        <div class='control-section'>
            <div class='section-title'>
                <span class='icon'>🎛️</span>
                Servo Motor Control (GPIO 18)
            </div>
            <div class='servo-controls'>
                <div class='slider-container'>
                    <div class='slider-label'>
                        <span>Angle:</span>
                        <span class='angle-display' id='angle-display'>90°</span>
                    </div>
                    <input type='range' id='servo-slider' min='0' max='180' value='90' 
                           oninput='updateAngleDisplay(this.value)'>
                </div>
                <button class='btn btn-rotate' onclick='rotateServo()'>Rotate to Position</button>
                
                <div class='slider-label' style='margin-top: 10px;'>
                    <span>Quick Positions:</span>
                </div>
                <div class='quick-angles'>
                    <button class='btn-quick' onclick='setAngle(0)'>0°</button>
                    <button class='btn-quick' onclick='setAngle(45)'>45°</button>
                    <button class='btn-quick' onclick='setAngle(90)'>90°</button>
                    <button class='btn-quick' onclick='setAngle(135)'>135°</button>
                    <button class='btn-quick' onclick='setAngle(180)'>180°</button>
                    <button class='btn-quick' onclick='sweepServo()'>Sweep</button>
                </div>
                <div id='servo-status' class='status'></div>
            </div>
        </div>
    </div>
    
    <script>
        function updateAngleDisplay(value) {
            document.getElementById('angle-display').textContent = value + '°';
        }
        
        function setAngle(angle) {
            document.getElementById('servo-slider').value = angle;
            updateAngleDisplay(angle);
            rotateServo();
        }
        
        async function controlLED(turnOn) {
            const statusDiv = document.getElementById('led-status');
            try {
                const response = await fetch(turnOn ? '/gpio/on' : '/gpio/off');
                if (response.ok) {
                    showStatus('led-status', `LED turned ${turnOn ? 'ON' : 'OFF'}!`, 'success');
                } else {
                    showStatus('led-status', 'Failed to control LED', 'error');
                }
            } catch (error) {
                showStatus('led-status', 'Error: ' + error.message, 'error');
            }
        }
        
        async function rotateServo() {
            const angle = document.getElementById('servo-slider').value;
            const statusDiv = document.getElementById('servo-status');
            
            try {
                showStatus('servo-status', `Rotating to ${angle}°...`, 'success');
                const response = await fetch(`/rotate/${angle}`);
                if (response.ok) {
                    showStatus('servo-status', `Servo rotated to ${angle}°!`, 'success');
                } else {
                    showStatus('servo-status', 'Failed to rotate servo', 'error');
                }
            } catch (error) {
                showStatus('servo-status', 'Error: ' + error.message, 'error');
            }
        }
        
        async function sweepServo() {
            showStatus('servo-status', 'Sweeping servo...', 'success');
            for (let angle = 0; angle <= 180; angle += 15) {
                document.getElementById('servo-slider').value = angle;
                updateAngleDisplay(angle);
                await fetch(`/rotate/${angle}`);
                await new Promise(resolve => setTimeout(resolve, 300));
            }
            showStatus('servo-status', 'Sweep complete!', 'success');
        }
        
        function showStatus(elementId, message, type) {
            const statusDiv = document.getElementById(elementId);
            statusDiv.textContent = message;
            statusDiv.className = 'status show ' + type;
            setTimeout(() => {
                statusDiv.classList.remove('show');
            }, 3000);
        }
    </script>
</body>
</html>",
        "text/html"));

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