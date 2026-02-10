namespace Shizuku.Core.Frames;

/// <summary>
/// Represents a captured VBUS ripple buffer from the CT-3 DSO mode.
/// The device captures into internal RAM then transfers as a block.
/// </summary>
public sealed class RippleBuffer
{
    /// <summary>Sample rate in samples per second (up to 3.2 Msps).</summary>
    public int SampleRateHz { get; init; }

    /// <summary>Number of valid samples in <see cref="Samples"/>.</summary>
    public int SampleCount { get; init; }

    /// <summary>Trigger level voltage that armed the capture.</summary>
    public double TriggerLevelV { get; init; }

    /// <summary>Index within <see cref="Samples"/> where the trigger occurred.</summary>
    public int TriggerIndex { get; init; }

    /// <summary>
    /// Raw AC-coupled voltage samples (volts).
    /// Length may exceed <see cref="SampleCount"/>; only the first
    /// <see cref="SampleCount"/> entries are valid.
    /// </summary>
    public double[] Samples { get; init; } = Array.Empty<double>();

    /// <summary>Time between samples in seconds.</summary>
    public double SamplePeriod => SampleRateHz > 0 ? 1.0 / SampleRateHz : 0;
}
