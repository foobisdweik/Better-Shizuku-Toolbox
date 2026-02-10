namespace Shizuku.Core.Crc;

/// <summary>
/// CRC-16 computation helper.
/// The actual polynomial will be determined during protocol reverse engineering.
/// This defaults to CRC-16/MODBUS (0xA001 reflected) as a common choice for embedded devices.
/// </summary>
public static class Crc16
{
    private const ushort Polynomial = 0xA001;
    private static readonly ushort[] Table = BuildTable();

    private static ushort[] BuildTable()
    {
        var table = new ushort[256];
        for (var i = 0; i < 256; i++)
        {
            ushort crc = (ushort)i;
            for (var j = 0; j < 8; j++)
            {
                crc = (crc & 1) != 0
                    ? (ushort)((crc >> 1) ^ Polynomial)
                    : (ushort)(crc >> 1);
            }

            table[i] = crc;
        }

        return table;
    }

    /// <summary>
    /// Compute CRC-16 over the given span.
    /// </summary>
    public static ushort Compute(ReadOnlySpan<byte> data)
    {
        ushort crc = 0xFFFF;
        foreach (var b in data)
        {
            crc = (ushort)((crc >> 8) ^ Table[(crc ^ b) & 0xFF]);
        }

        return crc;
    }

    /// <summary>
    /// Validate that the trailing two bytes of <paramref name="packetWithCrc"/>
    /// match the CRC of the preceding bytes (little-endian).
    /// </summary>
    public static bool Validate(ReadOnlySpan<byte> packetWithCrc)
    {
        if (packetWithCrc.Length < 3)
            return false;

        var payload = packetWithCrc[..^2];
        var expected = (ushort)(packetWithCrc[^2] | (packetWithCrc[^1] << 8));
        return Compute(payload) == expected;
    }
}
