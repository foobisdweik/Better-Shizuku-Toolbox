namespace Shizuku.Core;

/// <summary>
/// Known constants for the CT-3 binary wire protocol.
/// Values are placeholders to be confirmed via reverse engineering (Phase 0).
/// </summary>
public static class ProtocolConstants
{
    /// <summary>Likely packet preamble byte (unconfirmed).</summary>
    public const byte Preamble = 0xAA;

    /// <summary>Minimum valid packet length (preamble + cmd + length + crc).</summary>
    public const int MinPacketLength = 5;

    /// <summary>Default baud rate used by the CT-3 serial interface.</summary>
    public const int DefaultBaudRate = 115200;

    /// <summary>Default TCP port the toolbox server listens on.</summary>
    public const int DefaultTcpPort = 5005;

    // ----- Command IDs (placeholders until reverse-engineered) -----

    public const byte CmdStartTelemetry = 0x01;
    public const byte CmdStopTelemetry = 0x02;
    public const byte CmdRequestRipple = 0x10;
    public const byte CmdSetRippleConfig = 0x11;
    public const byte CmdRequestPdPackets = 0x20;
    public const byte CmdTriggerFastCharge = 0x30;
    public const byte CmdLuaUpload = 0x40;
    public const byte CmdLuaExecute = 0x41;
    public const byte CmdDeviceInfo = 0xF0;
}
