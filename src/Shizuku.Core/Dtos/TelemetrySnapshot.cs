namespace Shizuku.Core.Dtos;

/// <summary>
/// A UI-friendly snapshot of the latest telemetry state, including running statistics.
/// Produced by the pipeline for the ViewModel layer.
/// </summary>
public sealed class TelemetrySnapshot
{
    public double VoltageV { get; init; }
    public double CurrentA { get; init; }
    public double PowerW { get; init; }
    public double TemperatureC { get; init; }
    public double DPlusV { get; init; }
    public double DMinusV { get; init; }

    // Running statistics for the current session.
    public double MinVoltageV { get; init; }
    public double MaxVoltageV { get; init; }
    public double AvgVoltageV { get; init; }

    public double MinCurrentA { get; init; }
    public double MaxCurrentA { get; init; }
    public double AvgCurrentA { get; init; }

    public double AccumulatedMah { get; init; }
    public double AccumulatedMwh { get; init; }

    public long SampleCount { get; init; }
    public TimeSpan Elapsed { get; init; }
}
