using Shizuku.Core.Crc;

namespace Shizuku.Core.Tests.Crc;

[TestFixture]
public class Crc16Tests
{
    [Test]
    public void Compute_EmptySpan_ReturnsInitialValue()
    {
        var result = Crc16.Compute(ReadOnlySpan<byte>.Empty);
        // CRC-16/MODBUS initial value with no data processed should be 0xFFFF.
        Assert.That(result, Is.EqualTo((ushort)0xFFFF));
    }

    [Test]
    public void Compute_KnownInput_ReturnsExpectedCrc()
    {
        // CRC-16/MODBUS of "123456789" (ASCII bytes) = 0x4B37
        var data = "123456789"u8.ToArray();
        var result = Crc16.Compute(data);
        Assert.That(result, Is.EqualTo((ushort)0x4B37));
    }

    [Test]
    public void Validate_ValidPacket_ReturnsTrue()
    {
        var data = "123456789"u8.ToArray();
        var crc = Crc16.Compute(data);

        var packet = new byte[data.Length + 2];
        data.CopyTo(packet, 0);
        packet[^2] = (byte)(crc & 0xFF);
        packet[^1] = (byte)(crc >> 8);

        Assert.That(Crc16.Validate(packet), Is.True);
    }

    [Test]
    public void Validate_CorruptedPacket_ReturnsFalse()
    {
        var data = "123456789"u8.ToArray();
        var crc = Crc16.Compute(data);

        var packet = new byte[data.Length + 2];
        data.CopyTo(packet, 0);
        packet[^2] = (byte)(crc & 0xFF);
        packet[^1] = (byte)((crc >> 8) ^ 0xFF); // Flip bits.

        Assert.That(Crc16.Validate(packet), Is.False);
    }

    [Test]
    public void Validate_TooShort_ReturnsFalse()
    {
        Assert.That(Crc16.Validate(new byte[] { 0x01, 0x02 }), Is.False);
    }
}
