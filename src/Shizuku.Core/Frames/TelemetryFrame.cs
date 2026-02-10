namespace Shizuku.Core.Frames;

/// <summary>
/// Represents a single telemetry sample from the CT-3.
/// Fields mirror the 1 kHz data stream: voltage, current, power, and temperatures.
/// </summary>
public readonly struct TelemetryFrame
{
    /// <summary>Host-side timestamp when the frame was received (UTC ticks).</summary>
    public long TimestampTicks { get; init; }

    /// <summary>Voltage in volts (0-26 V, resolution 0.0001 V).</summary>
    public double VoltageV { get; init; }

    /// <summary>Current in amps (0-6 A, resolution 0.0001 A).</summary>
    public double CurrentA { get; init; }

    /// <summary>Power in watts (computed or reported by device).</summary>
    public double PowerW { get; init; }

    /// <summary>D+ voltage on the data lines (volts).</summary>
    public double DPlusV { get; init; }

    /// <summary>D- voltage on the data lines (volts).</summary>
    public double DMinusV { get; init; }

    /// <summary>Device temperature in degrees Celsius.</summary>
    public double TemperatureC { get; init; }

    /// <summary>Energy accumulated in milliampere-hours.</summary>
    public double AccumulatedMah { get; init; }

    /// <summary>Energy accumulated in milliwatt-hours.</summary>
    public double AccumulatedMwh { get; init; }

    public override string ToString() =>
        $"{VoltageV:F4}V  {CurrentA:F4}A  {PowerW:F4}W  {TemperatureC:F1}°C";
}
