using System;
using System.Diagnostics;
using Shizuku.Core;
using Shizuku.Core.Crc;
using Shizuku.Core.Parsers;
using Shizuku.Services.Decimation;

namespace Shizuku.Benchmarks;

internal static class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("=== Shizuku Benchmarks ===\n");

        BenchmarkPacketFramer();
        BenchmarkCrc16();
        BenchmarkMinMaxDecimator();
    }

    private static void BenchmarkPacketFramer()
    {
        Console.WriteLine("--- PacketFramer throughput ---");

        const int packetCount = 100_000;
        var payload = new byte[24]; // Simulated telemetry payload.
        var wirePackets = new byte[packetCount][];

        for (var i = 0; i < packetCount; i++)
        {
            wirePackets[i] = BuildPacket(ProtocolConstants.CmdStartTelemetry, payload);
        }

        // Concatenate all packets into a single stream.
        var totalLen = 0;
        foreach (var p in wirePackets) totalLen += p.Length;
        var stream = new byte[totalLen];
        var offset = 0;
        foreach (var p in wirePackets)
        {
            p.CopyTo(stream, offset);
            offset += p.Length;
        }

        var framer = new PacketFramer();
        var sw = Stopwatch.StartNew();

        var parsed = framer.Feed(stream);

        sw.Stop();

        Console.WriteLine($"  Packets:    {parsed.Count:N0} / {packetCount:N0}");
        Console.WriteLine($"  Bytes:      {totalLen:N0}");
        Console.WriteLine($"  Time:       {sw.Elapsed.TotalMilliseconds:F1} ms");
        Console.WriteLine($"  Throughput: {totalLen / sw.Elapsed.TotalSeconds / 1_000_000:F1} MB/s");
        Console.WriteLine();
    }

    private static void BenchmarkCrc16()
    {
        Console.WriteLine("--- CRC-16 throughput ---");

        const int iterations = 1_000_000;
        var data = new byte[32];
        new Random(42).NextBytes(data);

        var sw = Stopwatch.StartNew();

        for (var i = 0; i < iterations; i++)
        {
            Crc16.Compute(data);
        }

        sw.Stop();

        var totalBytes = (long)iterations * data.Length;
        Console.WriteLine($"  Iterations: {iterations:N0}");
        Console.WriteLine($"  Time:       {sw.Elapsed.TotalMilliseconds:F1} ms");
        Console.WriteLine($"  Throughput: {totalBytes / sw.Elapsed.TotalSeconds / 1_000_000:F1} MB/s");
        Console.WriteLine();
    }

    private static void BenchmarkMinMaxDecimator()
    {
        Console.WriteLine("--- MinMaxDecimator (10M points -> 1920 px) ---");

        const int sourceCount = 10_000_000;
        const int targetPixels = 1920;

        var source = new double[sourceCount];
        var rng = new Random(42);
        for (var i = 0; i < sourceCount; i++)
            source[i] = rng.NextDouble() * 5.0;

        var sw = Stopwatch.StartNew();

        var result = MinMaxDecimator.Decimate(source, targetPixels);

        sw.Stop();

        Console.WriteLine($"  Input:      {sourceCount:N0} samples");
        Console.WriteLine($"  Output:     {result.Length:N0} points");
        Console.WriteLine($"  Time:       {sw.Elapsed.TotalMilliseconds:F1} ms");
        Console.WriteLine($"  Rate:       {sourceCount / sw.Elapsed.TotalSeconds / 1_000_000:F1} M samples/s");
        Console.WriteLine();
    }

    /// <summary>Build a valid wire packet (same logic as tests).</summary>
    private static byte[] BuildPacket(byte cmdId, byte[] payload)
    {
        var len = (ushort)payload.Length;
        var packet = new byte[1 + 1 + 2 + payload.Length + 2];
        packet[0] = ProtocolConstants.Preamble;
        packet[1] = cmdId;
        packet[2] = (byte)(len & 0xFF);
        packet[3] = (byte)(len >> 8);
        payload.CopyTo(packet, 4);

        var crc = Crc16.Compute(packet.AsSpan(0, packet.Length - 2));
        packet[^2] = (byte)(crc & 0xFF);
        packet[^1] = (byte)(crc >> 8);

        return packet;
    }
}
