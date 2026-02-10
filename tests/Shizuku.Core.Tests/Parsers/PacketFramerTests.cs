using Shizuku.Core;
using Shizuku.Core.Crc;
using Shizuku.Core.Parsers;

namespace Shizuku.Core.Tests.Parsers;

[TestFixture]
public class PacketFramerTests
{
    private PacketFramer _framer = null!;

    [SetUp]
    public void SetUp()
    {
        _framer = new PacketFramer();
    }

    /// <summary>
    /// Build a valid wire packet:
    /// [Preamble 1B] [CmdId 1B] [PayloadLen 2B LE] [Payload NB] [CRC16 2B LE]
    /// CRC covers bytes from preamble through end of payload.
    /// </summary>
    private static byte[] BuildPacket(byte cmdId, byte[] payload)
    {
        var len = (ushort)payload.Length;
        var packet = new byte[1 + 1 + 2 + payload.Length + 2];
        packet[0] = ProtocolConstants.Preamble;
        packet[1] = cmdId;
        packet[2] = (byte)(len & 0xFF);
        packet[3] = (byte)(len >> 8);
        payload.CopyTo(packet, 4);

        var crc = Crc16.Compute(packet.AsSpan(0, packet.Length - 2));
        packet[^2] = (byte)(crc & 0xFF);
        packet[^1] = (byte)(crc >> 8);

        return packet;
    }

    [Test]
    public void Feed_ValidPacket_ReturnsOneResult()
    {
        var payload = new byte[] { 0x01, 0x02, 0x03 };
        var wire = BuildPacket(0x42, payload);

        var packets = _framer.Feed(wire);

        Assert.That(packets, Has.Count.EqualTo(1));
        Assert.That(packets[0].CommandId, Is.EqualTo(0x42));
        Assert.That(packets[0].CrcValid, Is.True);
        Assert.That(packets[0].Payload.ToArray(), Is.EqualTo(payload));
    }

    [Test]
    public void Feed_TwoPacketsInOneChunk_ReturnsBoth()
    {
        var p1 = BuildPacket(0x01, new byte[] { 0xAA });
        var p2 = BuildPacket(0x02, new byte[] { 0xBB, 0xCC });

        var combined = new byte[p1.Length + p2.Length];
        p1.CopyTo(combined, 0);
        p2.CopyTo(combined, p1.Length);

        var packets = _framer.Feed(combined);

        Assert.That(packets, Has.Count.EqualTo(2));
        Assert.That(packets[0].CommandId, Is.EqualTo(0x01));
        Assert.That(packets[1].CommandId, Is.EqualTo(0x02));
    }

    [Test]
    public void Feed_SplitAcrossCalls_ReassemblesPacket()
    {
        var wire = BuildPacket(0x10, new byte[] { 1, 2, 3, 4 });

        // Split in the middle.
        var half = wire.Length / 2;
        var part1 = wire.AsSpan(0, half);
        var part2 = wire.AsSpan(half);

        var r1 = _framer.Feed(part1);
        Assert.That(r1, Has.Count.EqualTo(0));

        var r2 = _framer.Feed(part2);
        Assert.That(r2, Has.Count.EqualTo(1));
        Assert.That(r2[0].CrcValid, Is.True);
    }

    [Test]
    public void Feed_GarbageBeforePreamble_SkipsToValidPacket()
    {
        var garbage = new byte[] { 0x00, 0xFF, 0x12 };
        var wire = BuildPacket(0x05, new byte[] { 0xDD });

        var combined = new byte[garbage.Length + wire.Length];
        garbage.CopyTo(combined, 0);
        wire.CopyTo(combined, garbage.Length);

        var packets = _framer.Feed(combined);

        Assert.That(packets, Has.Count.EqualTo(1));
        Assert.That(packets[0].CommandId, Is.EqualTo(0x05));
    }

    [Test]
    public void Feed_CorruptedCrc_FlagsCrcInvalid()
    {
        var wire = BuildPacket(0x01, new byte[] { 0x01 });
        wire[^1] ^= 0xFF; // Corrupt CRC.

        var packets = _framer.Feed(wire);

        Assert.That(packets, Has.Count.EqualTo(1));
        Assert.That(packets[0].CrcValid, Is.False);
    }

    [Test]
    public void Reset_ClearsPartialBuffer()
    {
        var wire = BuildPacket(0x01, new byte[] { 0x01 });
        _framer.Feed(wire.AsSpan(0, 3)); // Partial.
        _framer.Reset();

        // Feed the rest -- it should NOT produce a packet because buffer was cleared.
        var packets = _framer.Feed(wire.AsSpan(3));
        Assert.That(packets, Has.Count.EqualTo(0));
    }
}
