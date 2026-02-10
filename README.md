# Better-Shizuku-Toolbox

Cross-platform Shizuku CT-3 control suite (work in progress).

## Repository Layout

- `src/Shizuku.App` - Avalonia UI shell and views.
- `src/Shizuku.Core` - Protocol frames, parsers, DTOs, CRC helpers.
- `src/Shizuku.Hardware` - Transport abstraction and device discovery.
- `src/Shizuku.Services` - Pipelines, persistence, decimation.
- `tests/Shizuku.Core.Tests` - Protocol parser tests.
- `tools/Shizuku.Probe` - Phase 0 protocol probe CLI.
- `tools/Shizuku.Benchmarks` - Rendering and throughput benchmarks.
- `docs` - Protocol notes and capture artifacts.
- `.github/workflows` - CI workflows.