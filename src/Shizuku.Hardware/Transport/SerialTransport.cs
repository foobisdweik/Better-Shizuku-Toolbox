using System.IO.Ports;
using Shizuku.Core;

namespace Shizuku.Hardware.Transport;

/// <summary>
/// Serial port transport for direct USB CDC connection to the CT-3.
/// </summary>
public sealed class SerialTransport : IDeviceTransport
{
    private SerialPort? _port;
    private readonly string _portName;
    private readonly int _baudRate;

    public SerialTransport(string portName, int baudRate = ProtocolConstants.DefaultBaudRate)
    {
        _portName = portName;
        _baudRate = baudRate;
    }

    public bool IsOpen => _port?.IsOpen ?? false;

    public Task OpenAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        _port = new SerialPort(_portName, _baudRate)
        {
            ReadTimeout = 250,
            WriteTimeout = 250,
        };

        try
        {
            _port.Open();
        }
        catch (Exception ex)
        {
            _port.Dispose();
            _port = null;
            throw new InvalidOperationException($"Failed to open serial port {_portName}: {ex.Message}", ex);
        }

        return Task.CompletedTask;
    }

    public Task CloseAsync(CancellationToken ct = default)
    {
        if (_port is { IsOpen: true })
        {
            try { _port.Close(); } catch { /* best effort */ }
        }

        _port?.Dispose();
        _port = null;
        return Task.CompletedTask;
    }

    public Task<int> ReadAsync(Memory<byte> buffer, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        if (_port is not { IsOpen: true })
            throw new InvalidOperationException("Serial port is not open.");

        try
        {
            // SerialPort doesn't natively support Memory<byte>, so use the array path.
            var array = new byte[buffer.Length];
            var read = _port.Read(array, 0, array.Length);
            array.AsMemory(0, read).CopyTo(buffer);
            return Task.FromResult(read);
        }
        catch (TimeoutException)
        {
            return Task.FromResult(0);
        }
    }

    public Task WriteAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        if (_port is not { IsOpen: true })
            throw new InvalidOperationException("Serial port is not open.");

        var array = data.ToArray();
        _port.Write(array, 0, array.Length);
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        if (_port is { IsOpen: true })
        {
            try { _port.Close(); } catch { /* best effort */ }
        }

        _port?.Dispose();
        _port = null;
        return ValueTask.CompletedTask;
    }
}
