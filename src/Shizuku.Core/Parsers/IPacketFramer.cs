using Shizuku.Core.Frames;

namespace Shizuku.Core.Parsers;

/// <summary>
/// Extracts framed packets from a raw byte stream.
/// Handles preamble detection, length extraction, and CRC validation.
/// Implementations are stateful (they buffer partial data between calls).
/// </summary>
public interface IPacketFramer
{
    /// <summary>
    /// Feed raw bytes into the framer. Returns zero or more complete, validated packets.
    /// </summary>
    IReadOnlyList<RawPacket> Feed(ReadOnlySpan<byte> data);

    /// <summary>
    /// Discard any buffered partial packet data.
    /// </summary>
    void Reset();
}
