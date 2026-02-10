using Shizuku.Core.Frames;
using Shizuku.Hardware.Transport;

namespace Shizuku.Hardware;

/// <summary>
/// Manages the lifecycle of a connection to a CT-3 device.
/// Tracks connection state and device metadata.
/// </summary>
public sealed class SessionManager : IAsyncDisposable
{
    private IDeviceTransport? _transport;

    /// <summary>The currently connected transport, or null.</summary>
    public IDeviceTransport? Transport => _transport;

    /// <summary>Device metadata from the last successful handshake.</summary>
    public DeviceInfo? DeviceInfo { get; private set; }

    /// <summary>Whether a device is currently connected.</summary>
    public bool IsConnected => _transport?.IsOpen ?? false;

    /// <summary>
    /// Connect using the supplied transport.
    /// </summary>
    public async Task ConnectAsync(IDeviceTransport transport, CancellationToken ct = default)
    {
        // Close any existing connection first.
        await DisconnectAsync(ct);

        _transport = transport;
        await _transport.OpenAsync(ct);
    }

    /// <summary>
    /// Record device info obtained after a successful handshake.
    /// </summary>
    public void SetDeviceInfo(DeviceInfo info)
    {
        DeviceInfo = info;
    }

    /// <summary>
    /// Disconnect and dispose the current transport.
    /// </summary>
    public async Task DisconnectAsync(CancellationToken ct = default)
    {
        if (_transport is not null)
        {
            await _transport.CloseAsync(ct);
            await _transport.DisposeAsync();
            _transport = null;
        }

        DeviceInfo = null;
    }

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync();
    }
}
