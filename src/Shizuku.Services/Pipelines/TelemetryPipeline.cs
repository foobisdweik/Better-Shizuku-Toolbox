using System.Threading.Channels;
using Shizuku.Core.Frames;
using Shizuku.Core.Parsers;
using Shizuku.Hardware.Transport;

namespace Shizuku.Services.Pipelines;

/// <summary>
/// Producer-consumer pipeline that reads raw bytes from the device transport,
/// frames packets, parses telemetry, and publishes frames to downstream consumers.
/// Runs on a dedicated background thread to prevent UI blocking.
/// </summary>
public sealed class TelemetryPipeline : IAsyncDisposable
{
    private readonly IDeviceTransport _transport;
    private readonly IPacketFramer _framer;
    private readonly IFrameParser _parser;

    private readonly Channel<TelemetryFrame> _channel;
    private CancellationTokenSource? _cts;
    private Task? _readTask;

    /// <summary>Consumer side: read telemetry frames as they arrive.</summary>
    public ChannelReader<TelemetryFrame> Reader => _channel.Reader;

    /// <summary>Number of frames produced since the pipeline was started.</summary>
    public long FrameCount { get; private set; }

    public TelemetryPipeline(
        IDeviceTransport transport,
        IPacketFramer framer,
        IFrameParser parser,
        int channelCapacity = 8192)
    {
        _transport = transport;
        _framer = framer;
        _parser = parser;

        _channel = Channel.CreateBounded<TelemetryFrame>(new BoundedChannelOptions(channelCapacity)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleWriter = true,
            SingleReader = false,
        });
    }

    /// <summary>
    /// Start the background read loop.
    /// </summary>
    public void Start()
    {
        if (_readTask is not null)
            throw new InvalidOperationException("Pipeline is already running.");

        _cts = new CancellationTokenSource();
        _readTask = Task.Run(() => ReadLoopAsync(_cts.Token));
    }

    /// <summary>
    /// Stop the background read loop and complete the channel.
    /// </summary>
    public async Task StopAsync()
    {
        if (_cts is not null)
        {
            await _cts.CancelAsync();
        }

        if (_readTask is not null)
        {
            try { await _readTask; } catch (OperationCanceledException) { }
            _readTask = null;
        }

        _channel.Writer.TryComplete();
        _cts?.Dispose();
        _cts = null;
    }

    private async Task ReadLoopAsync(CancellationToken ct)
    {
        var buffer = new byte[4096];

        while (!ct.IsCancellationRequested)
        {
            try
            {
                var bytesRead = await _transport.ReadAsync(buffer, ct);
                if (bytesRead == 0)
                    continue;

                var packets = _framer.Feed(buffer.AsSpan(0, bytesRead));
                foreach (var packet in packets)
                {
                    if (!packet.CrcValid)
                        continue;

                    var frame = _parser.TryParseTelemetry(packet);
                    if (frame.HasValue)
                    {
                        _channel.Writer.TryWrite(frame.Value);
                        FrameCount++;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception)
            {
                // Transport error -- brief back-off then retry.
                try { await Task.Delay(100, ct); } catch { break; }
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
    }
}
