using System.Globalization;
using Shizuku.Core.Frames;

namespace Shizuku.Services.Persistence;

/// <summary>
/// Streams telemetry frames to a CSV file.
/// Writes are buffered and flushed periodically for performance.
/// </summary>
public sealed class CsvLogger : ICsvLogger
{
    private StreamWriter? _writer;
    private readonly object _lock = new();

    public bool IsLogging => _writer is not null;
    public long RowsWritten { get; private set; }

    public void Start(string filePath)
    {
        lock (_lock)
        {
            Stop(); // Close any existing file.

            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            _writer = new StreamWriter(filePath, append: false, encoding: System.Text.Encoding.UTF8)
            {
                AutoFlush = false,
            };

            _writer.WriteLine("Timestamp,Voltage_V,Current_A,Power_W,DPlus_V,DMinus_V,Temp_C,Mah,Mwh");
            RowsWritten = 0;
        }
    }

    public void Write(in TelemetryFrame frame)
    {
        lock (_lock)
        {
            if (_writer is null)
                return;

            var ts = new DateTime(frame.TimestampTicks, DateTimeKind.Utc);
            _writer.WriteLine(string.Create(CultureInfo.InvariantCulture,
                $"{ts:O},{frame.VoltageV:F4},{frame.CurrentA:F4},{frame.PowerW:F4}," +
                $"{frame.DPlusV:F4},{frame.DMinusV:F4},{frame.TemperatureC:F1}," +
                $"{frame.AccumulatedMah:F3},{frame.AccumulatedMwh:F3}"));

            RowsWritten++;

            // Periodic flush every 1000 rows.
            if (RowsWritten % 1000 == 0)
                _writer.Flush();
        }
    }

    public void Stop()
    {
        lock (_lock)
        {
            if (_writer is null)
                return;

            _writer.Flush();
            _writer.Dispose();
            _writer = null;
        }
    }

    public void Dispose()
    {
        Stop();
    }
}
