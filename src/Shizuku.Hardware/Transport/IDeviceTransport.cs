namespace Shizuku.Hardware.Transport;

/// <summary>
/// Abstraction over the physical link to a CT-3 device.
/// Implementations include serial (COM/ttyUSB) and TCP bridge.
/// </summary>
public interface IDeviceTransport : IAsyncDisposable
{
    /// <summary>Whether the transport is currently open and usable.</summary>
    bool IsOpen { get; }

    /// <summary>Open the transport. Throws on failure.</summary>
    Task OpenAsync(CancellationToken ct = default);

    /// <summary>Close the transport gracefully.</summary>
    Task CloseAsync(CancellationToken ct = default);

    /// <summary>
    /// Read available bytes into <paramref name="buffer"/>.
    /// Returns the number of bytes actually read (0 means no data available within timeout).
    /// </summary>
    Task<int> ReadAsync(Memory<byte> buffer, CancellationToken ct = default);

    /// <summary>
    /// Write bytes to the device.
    /// </summary>
    Task WriteAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default);
}
