# 📌 Pin Connection Guide

This document shows exactly which GPIO pins to use for each device in your Raspberry Pi Control Panel project.

## 🔌 Device Connections

### 💡 LED (GPIO 17)

| Component | Connection | Physical Pin | GPIO Pin |
|-----------|------------|--------------|----------|
| LED Anode (+) | Via 220Ω resistor | Pin 11 | GPIO 17 |
| LED Cathode (-) | Direct connection | Pin 6, 9, 14, 20, 25, 30, 34, or 39 | GND |

**Wiring:**
```
Raspberry Pi GPIO 17 (Pin 11) → 220Ω Resistor → LED Anode (+)
LED Cathode (-) → Raspberry Pi GND (any GND pin)
```

---

### 🎛️ Servo Motor (GPIO 18)

| Component | Connection | Physical Pin | GPIO Pin | Notes |
|-----------|------------|--------------|----------|-------|
| Signal Wire (Orange/Yellow) | Direct connection | Pin 12 | GPIO 18 | PWM control signal |
| Power Wire (Red) | 5V power | Pin 2 or 4 | 5V | ⚠️ Use external power for larger servos |
| Ground Wire (Brown/Black) | Direct connection | Pin 6, 9, 14, 20, 25, 30, 34, or 39 | GND | Common ground |

**Wiring:**
```
Raspberry Pi GPIO 18 (Pin 12) → Servo Signal Wire (Orange/Yellow)
Raspberry Pi 5V (Pin 2 or 4) → Servo Power Wire (Red) [Use external 5V for larger servos!]
Raspberry Pi GND (any GND pin) → Servo Ground Wire (Brown/Black)
```

---

## 🗺️ Raspberry Pi GPIO Pinout Reference

```
        3.3V [ 1] [ 2] 5V
   (SDA) GP2 [ 3] [ 4] 5V
   (SCL) GP3 [ 5] [ 6] GND
         GP4 [ 7] [ 8] GP14 (TXD)
         GND [ 9] [10] GP15 (RXD)
🔴 LED GP17 [11] [12] GP18 🎛️ SERVO
        GP27 [13] [14] GND
        GP22 [15] [16] GP23
        3.3V [17] [18] GP24
  (MOSI)GP10 [19] [20] GND
  (MISO)GP9  [21] [22] GP25
  (SCLK)GP11 [23] [24] GP8 (CE0)
         GND [25] [26] GP7 (CE1)
   (ID_SD)GP0[27] [28] GP1 (ID_SC)
        GP5  [29] [30] GND
        GP6  [31] [32] GP12
        GP13 [33] [34] GND
        GP19 [35] [36] GP16
        GP26 [37] [38] GP20
         GND [39] [40] GP21
```

---

## ⚡ Power & Ground Pins

### Power Pins (Do NOT use for GPIO control)
- **Pin 1, 17**: 3.3V (max 50mA total)
- **Pin 2, 4**: 5V (connect servo power here, or use external power supply)

### Ground Pins (GND) - All are equivalent
- **Pin 6, 9, 14, 20, 25, 30, 34, 39**: Ground (0V)

---

## 🛠️ Quick Setup Guide

### LED Setup
1. Take a 220Ω resistor (Red-Red-Brown or Red-Red-Black-Black bands)
2. Connect one end to **GPIO 17 (Physical Pin 11)**
3. Connect other end to LED's **longer leg (anode +)**
4. Connect LED's **shorter leg (cathode -)** to **GND (Pin 6 or any GND)**

### Servo Setup
1. Identify the three wires on your servo:
   - **Signal** (usually Orange or Yellow)
   - **Power** (usually Red)
   - **Ground** (usually Brown or Black)
2. Connect **Signal** wire to **GPIO 18 (Physical Pin 12)**
3. Connect **Power** wire to **5V (Pin 2 or 4)** OR external 5V power supply
4. Connect **Ground** wire to **GND (Pin 6 or any GND)**
5. If using external power, connect external GND to Raspberry Pi GND

---

## ⚠️ Important Safety Notes

1. **LED Protection**: Always use a resistor (220Ω recommended) with the LED to prevent damage
2. **Servo Power**: 
   - Small servos (SG90, 9g) can use the Pi's 5V pin
   - Larger servos should use an external 5V power supply
   - Always share common ground between Pi and external power
3. **Never connect 5V to GPIO pins** - GPIO pins are 3.3V tolerant only!
4. **Double-check connections** before powering on

---

## 🔍 Troubleshooting

### LED doesn't light up:
- ✅ Check LED polarity (longer leg = +, shorter leg = -)
- ✅ Verify GPIO 17 connection
- ✅ Test with multimeter or swap LED

### Servo doesn't move:
- ✅ Verify power connection (servo needs 5V)
- ✅ Check signal wire is on GPIO 18
- ✅ Ensure common ground connection
- ✅ Try external power supply if servo is large

### Application errors:
- ✅ Run with sudo: `sudo dotnet run`
- ✅ Check GPIO permissions: `sudo usermod -a -G gpio $USER`

---

## 📋 Summary Table

| Device | GPIO Pin | Physical Pin | Additional Components |
|--------|----------|--------------|----------------------|
| **LED** | GPIO 17 | Pin 11 | 220Ω resistor, GND connection |
| **Servo** | GPIO 18 | Pin 12 | 5V power, GND connection |

---

**Last Updated**: October 15, 2025  
**Project**: Raspberry Pi Control Panel  
**Repository**: Raspberry_Control
