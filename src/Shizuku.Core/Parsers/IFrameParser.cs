using Shizuku.Core.Frames;

namespace Shizuku.Core.Parsers;

/// <summary>
/// Parses a validated <see cref="RawPacket"/> into a strongly-typed domain frame.
/// </summary>
public interface IFrameParser
{
    /// <summary>
    /// Try to parse the raw packet into a telemetry frame.
    /// Returns null if the command ID does not correspond to telemetry.
    /// </summary>
    TelemetryFrame? TryParseTelemetry(in RawPacket packet);

    /// <summary>
    /// Try to parse the raw packet into a ripple buffer.
    /// Returns null if the command ID does not correspond to ripple data.
    /// </summary>
    RippleBuffer? TryParseRipple(in RawPacket packet);

    /// <summary>
    /// Try to parse the raw packet into a PD packet.
    /// Returns null if the command ID does not correspond to PD data.
    /// </summary>
    PdPacket? TryParsePdPacket(in RawPacket packet);

    /// <summary>
    /// Try to parse the raw packet into device info.
    /// Returns null if the command ID does not correspond to a device info response.
    /// </summary>
    DeviceInfo? TryParseDeviceInfo(in RawPacket packet);
}
