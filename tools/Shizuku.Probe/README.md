# Shizuku.Probe

Phase 0 probe tool for protocol reality checks.

## Behavior
- `list` enumerates serial ports.
- `read` captures raw bytes for a duration and optionally saves to disk.

## Usage
```
dotnet run --project tools/Shizuku.Probe -- list
dotnet run --project tools/Shizuku.Probe -- read --port COM3 --baud 115200 --duration 5 --output captures/session.bin
```

## Acceptance Criteria (Phase 0)
- Device appears in `list` output on at least one OS.
- `read` captures non-zero bytes from the device.
