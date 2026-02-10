namespace Shizuku.Core.Dtos;

/// <summary>
/// Configuration for a ripple capture request.
/// </summary>
public sealed class RippleConfig
{
    /// <summary>Desired sample rate in Hz (100 kHz to 3.2 MHz).</summary>
    public int SampleRateHz { get; init; } = 100_000;

    /// <summary>Number of samples to capture.</summary>
    public int SampleDepth { get; init; } = 4096;

    /// <summary>Trigger edge: true = rising, false = falling.</summary>
    public bool TriggerRisingEdge { get; init; } = true;

    /// <summary>Trigger level in volts.</summary>
    public double TriggerLevelV { get; init; }

    /// <summary>Trigger mode.</summary>
    public RippleTriggerMode TriggerMode { get; init; } = RippleTriggerMode.Auto;
}

public enum RippleTriggerMode
{
    Auto,
    Normal,
    Single,
}
