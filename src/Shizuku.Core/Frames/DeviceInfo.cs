namespace Shizuku.Core.Frames;

/// <summary>
/// Metadata returned by the CT-3 on initial handshake.
/// </summary>
public sealed class DeviceInfo
{
    /// <summary>Firmware version string reported by the device.</summary>
    public string FirmwareVersion { get; init; } = string.Empty;

    /// <summary>Serial number (if available).</summary>
    public string SerialNumber { get; init; } = string.Empty;

    /// <summary>Hardware revision identifier.</summary>
    public string HardwareRevision { get; init; } = string.Empty;

    /// <summary>Maximum supported telemetry sample rate (sps).</summary>
    public int MaxTelemetrySps { get; init; }

    /// <summary>Maximum supported ripple sample rate (sps).</summary>
    public int MaxRippleSps { get; init; }
}
