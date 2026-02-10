using Shizuku.Core.Frames;

namespace Shizuku.Services.Persistence;

/// <summary>
/// Writes telemetry frames to a CSV file in real time.
/// </summary>
public interface ICsvLogger : IDisposable
{
    /// <summary>Whether the logger is currently writing to a file.</summary>
    bool IsLogging { get; }

    /// <summary>Total rows written since the file was opened.</summary>
    long RowsWritten { get; }

    /// <summary>Begin logging to the specified file path.</summary>
    void Start(string filePath);

    /// <summary>Write a single telemetry frame as a CSV row.</summary>
    void Write(in TelemetryFrame frame);

    /// <summary>Flush and close the log file.</summary>
    void Stop();
}
