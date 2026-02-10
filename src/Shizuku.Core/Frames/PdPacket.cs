namespace Shizuku.Core.Frames;

/// <summary>
/// A decoded USB Power Delivery packet forwarded by the CT-3.
/// </summary>
public sealed class PdPacket
{
    /// <summary>Host-side timestamp when the packet was received (UTC ticks).</summary>
    public long TimestampTicks { get; init; }

    /// <summary>PD message type (e.g., SourceCapabilities, Request).</summary>
    public PdMessageType MessageType { get; init; }

    /// <summary>Whether this is a control or data message.</summary>
    public bool IsDataMessage { get; init; }

    /// <summary>Number of data objects in the packet.</summary>
    public int ObjectCount { get; init; }

    /// <summary>Raw 32-bit data objects (PDOs or RDOs).</summary>
    public uint[] DataObjects { get; init; } = Array.Empty<uint>();

    /// <summary>Raw packet bytes for hex display.</summary>
    public byte[] RawBytes { get; init; } = Array.Empty<byte>();
}

/// <summary>
/// Known USB PD message types (subset relevant to CT-3 sniffing).
/// </summary>
public enum PdMessageType : byte
{
    Unknown = 0,
    SourceCapabilities = 1,
    SinkCapabilities = 2,
    Request = 3,
    Accept = 4,
    Reject = 5,
    GoodCrc = 6,
    PsRdy = 7,
    GetSourceCap = 8,
    GetSinkCap = 9,
    SoftReset = 10,
    VendorDefined = 11,
}
