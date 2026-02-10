using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;

namespace Shizuku.Probe;

internal static class Program
{
    private static int Main(string[] args)
    {
        if (args.Length == 0 || IsHelp(args[0]))
        {
            PrintUsage();
            return 0;
        }

        var command = args[0].ToLowerInvariant();
        return command switch
        {
            "list" => ListPorts(),
            "read" => ReadRaw(args),
            _ => UnknownCommand(command)
        };
    }

    private static bool IsHelp(string value) =>
        value.Equals("help", StringComparison.OrdinalIgnoreCase) ||
        value.Equals("--help", StringComparison.OrdinalIgnoreCase) ||
        value.Equals("-h", StringComparison.OrdinalIgnoreCase);

    private static int UnknownCommand(string command)
    {
        Console.Error.WriteLine($"Unknown command '{command}'.");
        PrintUsage();
        return 1;
    }

    private static void PrintUsage()
    {
        Console.WriteLine("Shizuku.Probe");
        Console.WriteLine("Usage:");
        Console.WriteLine("  probe list");
        Console.WriteLine("  probe read --port COM3 --baud 115200 --duration 5 --output capture.bin");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  list  Enumerate available serial ports.");
        Console.WriteLine("  read  Read raw bytes from a port for a duration (seconds).");
    }

    private static int ListPorts()
    {
        var ports = SerialPort.GetPortNames();
        if (ports.Length == 0)
        {
            Console.WriteLine("No serial ports detected.");
            return 0;
        }

        Console.WriteLine("Detected serial ports:");
        foreach (var port in ports)
        {
            Console.WriteLine($"  {port}");
        }

        return 0;
    }

    private static int ReadRaw(string[] args)
    {
        var options = ParseOptions(args);
        if (!options.TryGetValue("port", out var portName))
        {
            Console.Error.WriteLine("Missing required --port option.");
            PrintUsage();
            return 1;
        }

        var baud = GetIntOption(options, "baud", 115200);
        var durationSeconds = GetIntOption(options, "duration", 5);
        options.TryGetValue("output", out var outputPath);

        if (!string.IsNullOrWhiteSpace(outputPath))
        {
            var fullPath = Path.GetFullPath(outputPath);
            var directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            outputPath = fullPath;
        }

        using var serialPort = new SerialPort(portName, baud)
        {
            ReadTimeout = 250
        };

        try
        {
            serialPort.Open();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to open port {portName}: {ex.Message}");
            return 1;
        }

        using var output = string.IsNullOrWhiteSpace(outputPath)
            ? Stream.Null
            : File.Open(outputPath, FileMode.Create, FileAccess.Write, FileShare.Read);

        var buffer = new byte[4096];
        var totalBytes = 0L;
        var stopwatch = Stopwatch.StartNew();
        var deadline = DateTime.UtcNow.AddSeconds(durationSeconds);

        Console.WriteLine($"Reading from {portName} @ {baud} baud for {durationSeconds}s...");

        while (DateTime.UtcNow < deadline)
        {
            try
            {
                var read = serialPort.Read(buffer, 0, buffer.Length);
                if (read > 0)
                {
                    output.Write(buffer, 0, read);
                    totalBytes += read;
                }
            }
            catch (TimeoutException)
            {
                // No data available yet.
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Read error: {ex.Message}");
                break;
            }
        }

        stopwatch.Stop();
        serialPort.Close();

        var seconds = Math.Max(1.0, stopwatch.Elapsed.TotalSeconds);
        var rate = totalBytes / seconds;
        Console.WriteLine($"Captured {totalBytes} bytes in {seconds:F1}s ({rate:F0} bytes/s).");

        if (!string.IsNullOrWhiteSpace(outputPath))
        {
            Console.WriteLine($"Saved to {outputPath}.");
        }

        return 0;
    }

    private static Dictionary<string, string> ParseOptions(string[] args)
    {
        var options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 1; i < args.Length; i++)
        {
            var arg = args[i];
            if (!arg.StartsWith("--", StringComparison.Ordinal))
            {
                continue;
            }

            var key = arg[2..];
            var value = "true";
            if (i + 1 < args.Length && !args[i + 1].StartsWith("--", StringComparison.Ordinal))
            {
                value = args[i + 1];
                i++;
            }

            options[key] = value;
        }

        return options;
    }

    private static int GetIntOption(Dictionary<string, string> options, string key, int fallback)
    {
        if (options.TryGetValue(key, out var raw) && int.TryParse(raw, out var value))
        {
            return value;
        }

        return fallback;
    }
}
