using System.Buffers.Binary;
using Shizuku.Core;
using Shizuku.Core.Frames;
using Shizuku.Core.Parsers;

namespace Shizuku.Core.Tests.Parsers;

[TestFixture]
public class FrameParserTests
{
    private FrameParser _parser = null!;

    [SetUp]
    public void SetUp()
    {
        _parser = new FrameParser();
    }

    [Test]
    public void TryParseTelemetry_WrongCommandId_ReturnsNull()
    {
        var packet = new RawPacket
        {
            CommandId = 0xFF,
            Payload = new byte[24],
            CrcValid = true,
        };

        Assert.That(_parser.TryParseTelemetry(packet), Is.Null);
    }

    [Test]
    public void TryParseTelemetry_ValidPayload_ParsesValues()
    {
        var payload = new byte[24];
        BinaryPrimitives.WriteInt16LittleEndian(payload.AsSpan(0), 12345);  // 1.2345V
        BinaryPrimitives.WriteInt16LittleEndian(payload.AsSpan(2), 5432);   // 0.5432A
        BinaryPrimitives.WriteInt16LittleEndian(payload.AsSpan(4), 6700);   // 0.6700W
        BinaryPrimitives.WriteInt16LittleEndian(payload.AsSpan(6), 0);      // D+
        BinaryPrimitives.WriteInt16LittleEndian(payload.AsSpan(8), 0);      // D-
        BinaryPrimitives.WriteInt16LittleEndian(payload.AsSpan(10), 352);   // 35.2C
        BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(12), 1500);  // 1.5 mAh
        BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(16), 7500);  // 7.5 mWh

        var packet = new RawPacket
        {
            CommandId = ProtocolConstants.CmdStartTelemetry,
            Payload = payload,
            CrcValid = true,
        };

        var frame = _parser.TryParseTelemetry(packet);

        Assert.That(frame, Is.Not.Null);
        Assert.That(frame!.Value.VoltageV, Is.EqualTo(1.2345).Within(0.001));
        Assert.That(frame.Value.CurrentA, Is.EqualTo(0.5432).Within(0.001));
        Assert.That(frame.Value.TemperatureC, Is.EqualTo(35.2).Within(0.1));
        Assert.That(frame.Value.AccumulatedMah, Is.EqualTo(1.5).Within(0.01));
    }

    [Test]
    public void TryParseTelemetry_PayloadTooShort_ReturnsNull()
    {
        var packet = new RawPacket
        {
            CommandId = ProtocolConstants.CmdStartTelemetry,
            Payload = new byte[10], // Too short (needs 24).
            CrcValid = true,
        };

        Assert.That(_parser.TryParseTelemetry(packet), Is.Null);
    }

    [Test]
    public void TryParseDeviceInfo_ValidPayload_ParsesFirmwareVersion()
    {
        var fwBytes = System.Text.Encoding.ASCII.GetBytes("v1.2.3\0");
        var payload = new byte[8 + fwBytes.Length];
        BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(0), 1000);     // max telemetry
        BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(4), 3200000);  // max ripple
        fwBytes.CopyTo(payload, 8);

        var packet = new RawPacket
        {
            CommandId = ProtocolConstants.CmdDeviceInfo,
            Payload = payload,
            CrcValid = true,
        };

        var info = _parser.TryParseDeviceInfo(packet);

        Assert.That(info, Is.Not.Null);
        Assert.That(info!.FirmwareVersion, Is.EqualTo("v1.2.3"));
        Assert.That(info.MaxTelemetrySps, Is.EqualTo(1000));
        Assert.That(info.MaxRippleSps, Is.EqualTo(3200000));
    }
}
