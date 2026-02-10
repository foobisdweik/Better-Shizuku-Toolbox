using System.Net;
using System.Net.Sockets;
using Shizuku.Core;

namespace Shizuku.Hardware.Transport;

/// <summary>
/// TCP transport for the "Shizuku Toolbox" TCP bridge mode.
/// The application acts as a TCP server; the device connects through the USB pipe.
/// </summary>
public sealed class TcpBridgeTransport : IDeviceTransport
{
    private TcpListener? _listener;
    private TcpClient? _client;
    private NetworkStream? _stream;
    private readonly int _port;

    public TcpBridgeTransport(int port = ProtocolConstants.DefaultTcpPort)
    {
        _port = port;
    }

    public bool IsOpen => _stream is not null && _client is { Connected: true };

    public async Task OpenAsync(CancellationToken ct = default)
    {
        _listener = new TcpListener(IPAddress.Loopback, _port);
        _listener.Start();

        try
        {
            _client = await _listener.AcceptTcpClientAsync(ct);
            _stream = _client.GetStream();
        }
        catch
        {
            _listener.Stop();
            _listener = null;
            throw;
        }
    }

    public Task CloseAsync(CancellationToken ct = default)
    {
        _stream?.Dispose();
        _stream = null;
        _client?.Dispose();
        _client = null;
        _listener?.Stop();
        _listener = null;
        return Task.CompletedTask;
    }

    public async Task<int> ReadAsync(Memory<byte> buffer, CancellationToken ct = default)
    {
        if (_stream is null)
            throw new InvalidOperationException("TCP bridge is not connected.");

        return await _stream.ReadAsync(buffer, ct);
    }

    public async Task WriteAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default)
    {
        if (_stream is null)
            throw new InvalidOperationException("TCP bridge is not connected.");

        await _stream.WriteAsync(data, ct);
    }

    public async ValueTask DisposeAsync()
    {
        await CloseAsync();
    }
}
