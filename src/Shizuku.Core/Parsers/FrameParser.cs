using System.Buffers.Binary;
using Shizuku.Core.Frames;

namespace Shizuku.Core.Parsers;

/// <summary>
/// Default implementation of <see cref="IFrameParser"/>.
/// Payload layout is speculative and will be refined during protocol reverse engineering.
/// </summary>
public sealed class FrameParser : IFrameParser
{
    public TelemetryFrame? TryParseTelemetry(in RawPacket packet)
    {
        if (packet.CommandId != ProtocolConstants.CmdStartTelemetry)
            return null;

        var span = packet.Payload.Span;
        if (span.Length < 24)
            return null;

        // Speculative layout: 8 x int16 scaled values (0.0001 V/A resolution).
        return new TelemetryFrame
        {
            TimestampTicks = DateTime.UtcNow.Ticks,
            VoltageV = BinaryPrimitives.ReadInt16LittleEndian(span[0..]) / 10000.0,
            CurrentA = BinaryPrimitives.ReadInt16LittleEndian(span[2..]) / 10000.0,
            PowerW = BinaryPrimitives.ReadInt16LittleEndian(span[4..]) / 10000.0,
            DPlusV = BinaryPrimitives.ReadInt16LittleEndian(span[6..]) / 10000.0,
            DMinusV = BinaryPrimitives.ReadInt16LittleEndian(span[8..]) / 10000.0,
            TemperatureC = BinaryPrimitives.ReadInt16LittleEndian(span[10..]) / 10.0,
            AccumulatedMah = BinaryPrimitives.ReadInt32LittleEndian(span[12..]) / 1000.0,
            AccumulatedMwh = BinaryPrimitives.ReadInt32LittleEndian(span[16..]) / 1000.0,
        };
    }

    public RippleBuffer? TryParseRipple(in RawPacket packet)
    {
        if (packet.CommandId != ProtocolConstants.CmdRequestRipple)
            return null;

        var span = packet.Payload.Span;
        if (span.Length < 12)
            return null;

        var sampleRate = BinaryPrimitives.ReadInt32LittleEndian(span[0..]);
        var sampleCount = BinaryPrimitives.ReadInt32LittleEndian(span[4..]);
        var triggerIndex = BinaryPrimitives.ReadInt32LittleEndian(span[8..]);

        // Each sample is 2 bytes (int16), starting at offset 12.
        var expectedDataLen = sampleCount * 2;
        if (span.Length < 12 + expectedDataLen)
            return null;

        var samples = new double[sampleCount];
        for (var i = 0; i < sampleCount; i++)
        {
            var raw = BinaryPrimitives.ReadInt16LittleEndian(span[(12 + i * 2)..]);
            samples[i] = raw / 10000.0; // Scale TBD.
        }

        return new RippleBuffer
        {
            SampleRateHz = sampleRate,
            SampleCount = sampleCount,
            TriggerIndex = triggerIndex,
            Samples = samples,
        };
    }

    public PdPacket? TryParsePdPacket(in RawPacket packet)
    {
        if (packet.CommandId != ProtocolConstants.CmdRequestPdPackets)
            return null;

        var span = packet.Payload.Span;
        if (span.Length < 4)
            return null;

        var msgType = (PdMessageType)span[0];
        var isData = span[1] != 0;
        var objCount = span[2];

        var dataObjects = new uint[objCount];
        var dataStart = 4;
        for (var i = 0; i < objCount && dataStart + 4 <= span.Length; i++, dataStart += 4)
        {
            dataObjects[i] = BinaryPrimitives.ReadUInt32LittleEndian(span[dataStart..]);
        }

        return new PdPacket
        {
            TimestampTicks = DateTime.UtcNow.Ticks,
            MessageType = msgType,
            IsDataMessage = isData,
            ObjectCount = objCount,
            DataObjects = dataObjects,
            RawBytes = packet.Payload.ToArray(),
        };
    }

    public DeviceInfo? TryParseDeviceInfo(in RawPacket packet)
    {
        if (packet.CommandId != ProtocolConstants.CmdDeviceInfo)
            return null;

        var span = packet.Payload.Span;
        if (span.Length < 8)
            return null;

        // Speculative layout: first 4 bytes = max telemetry sps, next 4 = max ripple sps,
        // remainder = null-terminated firmware version string.
        var maxTelemetry = BinaryPrimitives.ReadInt32LittleEndian(span[0..]);
        var maxRipple = BinaryPrimitives.ReadInt32LittleEndian(span[4..]);

        var fwBytes = span[8..];
        var nullIdx = fwBytes.IndexOf((byte)0);
        var fwVersion = nullIdx >= 0
            ? System.Text.Encoding.ASCII.GetString(fwBytes[..nullIdx])
            : System.Text.Encoding.ASCII.GetString(fwBytes);

        return new DeviceInfo
        {
            FirmwareVersion = fwVersion,
            MaxTelemetrySps = maxTelemetry,
            MaxRippleSps = maxRipple,
        };
    }
}
