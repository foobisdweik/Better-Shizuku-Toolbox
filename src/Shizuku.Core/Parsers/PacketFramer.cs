using Shizuku.Core.Crc;
using Shizuku.Core.Frames;

namespace Shizuku.Core.Parsers;

/// <summary>
/// Default implementation of <see cref="IPacketFramer"/>.
/// Scans for the preamble byte, reads the length, validates CRC,
/// and emits complete <see cref="RawPacket"/> instances.
///
/// Expected wire format (to be confirmed via reverse engineering):
///   [Preamble 1B] [CommandId 1B] [PayloadLength 2B LE] [Payload NB] [CRC16 2B LE]
/// </summary>
public sealed class PacketFramer : IPacketFramer
{
    private readonly byte[] _buffer = new byte[65536];
    private int _writePos;

    public IReadOnlyList<RawPacket> Feed(ReadOnlySpan<byte> data)
    {
        // Append incoming data to internal buffer.
        if (_writePos + data.Length > _buffer.Length)
        {
            // Overflow protection: discard and resync.
            _writePos = 0;
        }

        data.CopyTo(_buffer.AsSpan(_writePos));
        _writePos += data.Length;

        var results = new List<RawPacket>();
        var readPos = 0;

        while (readPos < _writePos)
        {
            // Scan for preamble.
            if (_buffer[readPos] != ProtocolConstants.Preamble)
            {
                readPos++;
                continue;
            }

            var remaining = _writePos - readPos;
            if (remaining < ProtocolConstants.MinPacketLength)
                break; // Need more data.

            var cmdId = _buffer[readPos + 1];
            var payloadLen = (ushort)(_buffer[readPos + 2] | (_buffer[readPos + 3] << 8));

            // Total packet: preamble(1) + cmd(1) + lenField(2) + payload(N) + crc(2)
            var totalLen = 1 + 1 + 2 + payloadLen + 2;
            if (remaining < totalLen)
                break; // Need more data.

            var packetSpan = new ReadOnlySpan<byte>(_buffer, readPos, totalLen);

            // CRC covers everything except the trailing 2 CRC bytes.
            var crcValid = Crc16.Validate(packetSpan);

            var payloadBytes = new byte[payloadLen];
            packetSpan.Slice(4, payloadLen).CopyTo(payloadBytes);

            results.Add(new RawPacket
            {
                CommandId = cmdId,
                Payload = payloadBytes,
                CrcValid = crcValid,
            });

            readPos += totalLen;
        }

        // Compact: shift unconsumed bytes to the front of the buffer.
        if (readPos > 0)
        {
            var leftover = _writePos - readPos;
            if (leftover > 0)
            {
                Buffer.BlockCopy(_buffer, readPos, _buffer, 0, leftover);
            }

            _writePos = leftover;
        }

        return results;
    }

    public void Reset()
    {
        _writePos = 0;
    }
}
