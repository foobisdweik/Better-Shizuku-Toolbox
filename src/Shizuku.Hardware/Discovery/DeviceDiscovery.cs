using System.IO.Ports;
using System.Runtime.InteropServices;

namespace Shizuku.Hardware.Discovery;

/// <summary>
/// Enumerates serial ports and filters for likely CT-3 devices.
/// Cross-platform: handles both Windows (COM*) and Linux (/dev/tty*) paths.
/// </summary>
public static class DeviceDiscovery
{
    /// <summary>
    /// Returns all serial port names available on the system.
    /// </summary>
    public static string[] GetAllPorts()
    {
        return SerialPort.GetPortNames();
    }

    /// <summary>
    /// Returns serial ports that are likely to be CT-3 devices.
    /// On Linux, filters for /dev/ttyUSB* and /dev/ttyACM*.
    /// On Windows, returns all COM ports (further filtering requires probing).
    /// </summary>
    public static IReadOnlyList<string> GetCandidatePorts()
    {
        var ports = GetAllPorts();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return ports
                .Where(p => p.StartsWith("/dev/ttyUSB", StringComparison.Ordinal) ||
                            p.StartsWith("/dev/ttyACM", StringComparison.Ordinal))
                .OrderBy(p => p, StringComparer.Ordinal)
                .ToList();
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return ports
                .Where(p => p.StartsWith("COM", StringComparison.OrdinalIgnoreCase))
                .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        // macOS or unknown: return all.
        return ports.OrderBy(p => p, StringComparer.Ordinal).ToList();
    }
}
