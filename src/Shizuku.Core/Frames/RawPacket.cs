namespace Shizuku.Core.Frames;

/// <summary>
/// A raw, validated packet extracted from the byte stream before semantic parsing.
/// </summary>
public readonly struct RawPacket
{
    /// <summary>Command / message ID byte.</summary>
    public byte CommandId { get; init; }

    /// <summary>Payload bytes (excluding preamble, length, and CRC).</summary>
    public ReadOnlyMemory<byte> Payload { get; init; }

    /// <summary>Whether the CRC check passed.</summary>
    public bool CrcValid { get; init; }
}
