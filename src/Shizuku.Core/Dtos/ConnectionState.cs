namespace Shizuku.Core.Dtos;

/// <summary>
/// Represents the current device connection state for the status bar.
/// </summary>
public sealed class ConnectionState
{
    public bool IsConnected { get; init; }
    public string PortName { get; init; } = string.Empty;
    public int BaudRate { get; init; }
    public string DeviceFirmware { get; init; } = string.Empty;
    public string StatusText { get; init; } = "Disconnected";
}
