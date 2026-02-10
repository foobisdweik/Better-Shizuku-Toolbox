Markdown Preview
Copy
---
url: https://pastebin.com/raw/p3azGm5U
title: ""
date: 2026-02-08T13:01:34.779Z
lang: en-US
---

Comprehensive Engineering Strategy for the Development of a Cross-Platform Shizuku CT-3 Control Suite
1. Strategic Context and Hardware Architecture Analysis
The landscape of embedded systems instrumentation has evolved rapidly, with USB-based multimeters transitioning from simple voltage checkers to sophisticated logic analyzers and oscilloscopes. The Shizuku CT-3—also distributed under the nomenclature AVHzY CT-3, Power-Z KT002, and Atorch UT18—represents a pinnacle in this niche, offering capabilities that rival benchtop equipment.1 However, the accompanying software ecosystem, specifically the "Shizuku System Box," remains tethered to legacy Windows frameworks, limiting its utility in modern, multi-platform engineering environments.4 This report outlines a definitive architectural blueprint for a next-generation, cross-platform control application, leveraging modern.NET technologies and AI-assisted development workflows to achieve strict feature parity and superior user experience.
1.1 The Hardware Paradigm: Capabilities and Constraints
To re-engineer the control software, one must first dissect the hardware's operational parameters. The CT-3 is not merely a passive logger; it is an active signal processing unit built around a high-performance microcontroller unit (MCU). The device features a 1.77-inch TFT display and dual USB-C ports (input/output) capable of passing through 100W+ PD power while sniffing the Communication Channel (CC) lines.3
The defining technical specifications that drive software requirements include the Analog-to-Digital Converter (ADC) performance and the sampling architecture. The device supports a standard voltage measurement range of 0-26V and a current range of 0-6A, with a measurement resolution of 0.0001V and 0.0001A respectively.3 Crucially, the hardware supports two distinct data acquisition modes that the software must handle concurrently: a standard telemetry stream and a high-speed ripple capture.
The telemetry stream operates at a maximum PC sampling rate of 1000 samples per second (sps).3 This data stream aggregates voltage, current, power, and temperature readings. While 1000 sps is trivial for modern processors, ensuring jitter-free visualization of this stream requires a decoupled rendering architecture.
In contrast, the "VBUS Ripple" feature—effectively a Digital Storage Oscilloscope (DSO) mode—samples the AC-coupled voltage at rates up to 3.2 Mega-samples per second (Msps).3 This capability presents the primary engineering challenge. USB Virtual COM ports (CDC class) often struggle to sustain continuous throughput at these rates without packet loss or significant host-side buffering. The software architecture must therefore support a "burst and buffer" acquisition model, where the device captures a finite depth of samples into its internal RAM and transfers them asynchronously to the host, rather than attempting a real-time continuous stream which would exceed the bandwidth of the serial interface implementation typically found on these MCUs.
1.2 The Software Gap: Limitations of the Legacy System
The existing "Shizuku System Box" software provides a functional but antiquated interface. It relies on a proprietary binary protocol and is compiled exclusively for the Windows operating system, likely using Windows Forms or WPF.5 This exclusion of macOS and Linux (Debian/Ubuntu) is a critical deficiency for firmware engineers and hardware developers who operate primarily in Unix-like environments.
Furthermore, the original software's architecture tightly couples the user interface thread with data acquisition routines. Users have reported that heavy logging loads or high-speed ripple rendering can cause UI unresponsiveness. The monolithic design also obfuscates the communication protocol, requiring users to rely on opaque binaries rather than open standards. The proposed replacement application will address these issues by adopting a modular, asynchronous architecture that strictly separates the data model from the view logic.
1.3 Project Scope and Objectives
The objective is to deliver a GUI-driven application that replaces the Shizuku System Box in its entirety. This is not a partial wrapper but a complete functional substitute.

Feature Domain
Target Capability
Core Telemetry
1000 sps logging of V, A, W, T with real-time charting.3
Oscilloscope
AC-coupled ripple visualization at 3.2 Msps with trigger controls.8
Protocol Analysis
USB PD 3.0/3.1 listener with packet decoding and PDO inspection.4
Fast Charge
Trigger controls for QC, PD, PPS, SCP, FCP with safety interlocks.7
Scripting
Integrated Lua editor and execution console for on-device automation.9
Platform
Native execution on Windows 10/11, macOS 12+, and Linux (x64/ARM64).

The development methodology will integrate advanced AI coding assistants—Codium, Cursor Pro, and Claude—to accelerate the reverse-engineering of the wire protocol and the generation of boilerplate UI code. This "AI-First" approach demands a rigorous prompt engineering strategy to ensure the generated code adheres to safety-critical standards required for hardware interfacing.
2. Protocol Engineering and Signal Interface
The most significant barrier to entry for this project is the proprietary nature of the communication protocol. The CT-3 utilizes a sophisticated, albeit undocumented, method of data exchange that toggles between standard serial communication and a bridged TCP socket mode.
2.1 The Hybrid Communication Model
Research indicates that the CT-3 does not rely solely on a standard text-based SCPI (Standard Commands for Programmable Instruments) interface. Instead, it employs a custom binary protocol over a virtual serial port. However, a critical discovery in the documentation for the device's Lua API reveals a secondary communication vector: the "Shizuku Toolbox" acts as a TCP server.10
In this architecture, the legacy software opens a TCP port (typically communicating with localhost). The meter, via its USB connection, effectively "mounts" this TCP connection, allowing the Lua scripts running on the device to send data to the PC application as if it were a network peer. This tcp module functionality 10 suggests that the replacement software must implement a TCP Server capable of handshaking with the device. This abstraction layer is powerful; it implies that the core application logic can be decoupled from the physical transport layer. The application can simply listen on a socket, while a lightweight "driver" service handles the USB-to-TCP encapsulation.
2.2 Reverse Engineering Strategy
To fully map the command set, a multi-modal reverse engineering campaign is required. The AI tools will be instrumental here, not just for code generation, but for pattern recognition in data dumps.
The primary attack vector involves "sniffing" the USB traffic while the legacy software performs specific actions. Using tools like Wireshark with USBPcap 11 or a Serial Port Monitor 5, we can isolate the command packets. For example, initiating a "Read Voltage" command in the official software will generate a specific sequence of bytes on the wire.
The AI-assisted workflow for this phase is as follows:
Data Capture: A human operator performs a discrete action (e.g., set ripple sample rate to 100kHz) and captures the serial traffic.
Pattern Isolation: The captured hex dump is fed into Claude or Cursor Pro.
Heuristic Analysis: The AI is prompted to identify the header (likely a static preamble like 0xAA), the command ID (the byte that changes between different actions), the payload length, and the checksum algorithm (CRC16 or XOR).
Mock Implementation: Codium is used to generate a Python script that replays the sequence to the device to confirm the behavior.
This "stimulus-response" analysis must be repeated for every feature: connecting the meter, starting the datalogger, requesting the ripple buffer, and triggering a fast charge protocol.
2.3 Throughput and Data Marshaling
The handling of the 3.2 Msps ripple data presents a specific bottleneck. A standard UART interface, even over USB, often caps at effective throughputs of 1-2 Mbps depending on the driver implementation.13 Transmitting 3.2 million samples per second, where each sample is likely 12-16 bits (2 bytes), would require a bandwidth of ~6.4 MB/s (approx 51 Mbps). This exceeds standard virtual COM port capabilities.
Therefore, the protocol likely utilizes a "Block Transfer" or "Bulk Read" mechanism. The software sends a trigger command, the device fills its internal RAM buffer with high-speed samples, and then transmits the buffer to the PC at a slower, reliable rate.8 The replacement software's RippleService must handle this state machine:
State Idle: Wait for user trigger.
State Arm: Send configuration (Sample Rate, Depth, Trigger Voltage) to the device.
State Wait: Poll the device for "Buffer Ready" status.
State Transfer: Read the binary blob from the stream.
State Render: Process the blob into double arrays for the oscilloscope view.
This asynchronous state machine prevents the UI from freezing while waiting for the hardware to capture the transient event.
3. Software Architecture and Technology Stack
To satisfy the requirement for cross-platform compatibility without sacrificing the performance needed for high-frequency plotting, a specific set of technologies has been selected. The architecture follows a strict Model-View-ViewModel (MVVM) pattern, ensuring separation of concerns and testability.
3.1 Core Framework:.NET 8 and Avalonia UI
The application will be built on .NET 8, the latest Long Term Support (LTS) release of the platform..NET 8 offers significant performance improvements in the JIT compiler and garbage collector, which are crucial for processing high-frequency telemetry data.
For the User Interface, Avalonia UI is the definitive choice.14 Unlike Electron, which bundles a resource-heavy Chromium instance, or MAUI, which relies on platform-specific native controls that can behave inconsistently, Avalonia renders its own pixels using the Skia graphics engine.16 This ensures that the oscilloscope graphs and dashboard widgets look and behave exactly the same on a Windows 11 workstation, a macOS MacBook, and a Raspberry Pi (Linux) in a lab setting.
Avalonia's XAML-based styling system allows for pixel-perfect control over the technical dashboard aesthetic required for engineering tools. It supports hardware acceleration via GPU, which is essential for rendering smooth scrolling charts at 60 FPS while the background data ingestion thread processes 1000 incoming packets per second.18
3.2 High-Performance Charting: ScottPlot 5.0
The "Oscilloscope" and "Data Logger" views require a charting library capable of rendering millions of points without lag. ScottPlot 5.0 is selected for this role.19 It is an open-source library specifically optimized for scientific and engineering applications. Unlike general-purpose charting libraries that prioritize animations and "business" aesthetics, ScottPlot prioritizes rendering speed and interactive performance (pan/zoom) for dense signal data.
For the VBUS Ripple feature, ScottPlot's "Signal Plot" mode will be utilized. This mode is optimized for evenly spaced data (like an oscilloscope trace), where the X-axis is calculated rather than stored, reducing memory overhead significantly compared to X/Y scatter plots.21
3.3 Application Layering (MVVM)
The architecture is divided into three distinct layers to manage complexity:
The Model Layer (Hardware Abstraction):
This layer contains the "Drivers." It has no knowledge of the UI.
ISerialInterface: An abstraction over System.IO.Ports or LibUsbDotNet that handles the raw byte I/O.
ShizukuProtocolInterpreter: A stateless parser that accepts byte arrays and returns strongly-typed objects (e.g., TelemetryFrame, RippleBuffer).
SessionManager: Manages the connection state, keeping track of the connected device's metadata (Firmware Version, Serial Number).
The ViewModel Layer (Application Logic):
This layer transforms raw data into observable properties for the UI.
MainViewModel: The conductor that manages navigation between views.
LoggerViewModel: Maintains a ObservableCircularBuffer of the last N seconds of telemetry. It calculates statistics (Min/Max/Avg) and handles the "Auto-Start/Stop" logic defined in the requirements.4
ScriptEditorViewModel: Manages the text buffer for the Lua editor and handles file I/O operations to save scripts to the local disk or upload them to the device.9
The View Layer (Presentation):
MainWindow: The primary shell defined in Avalonia XAML.
DashboardView: A composite view containing the digital readouts and the main trend chart.
RippleScopeView: A specialized view for the oscilloscope, containing the trigger controls and the high-speed waveform display.
3.4 Concurrency and Data Pipelining
Handling the disparate data rates of the telemetry logging (1 kHz) and the UI rendering (60 Hz) requires a Producer-Consumer pattern.
The SerialDataService (Producer) runs on a high-priority background thread. It reads bytes from the hardware as fast as possible to prevent buffer overflows on the FTDI/serial chip. These bytes are parsed into Measurement structs and placed into a thread-safe collection, such as a ConcurrentQueue or a Channel<T>.
The UIUpdateService (Consumer) runs on a timer synced to the display refresh rate (approx. 16ms). On every tick, it dequeues all available measurements from the queue. It processes them for statistics (e.g., updating the "Max Voltage" value) and then downsamples or aggregates them for the chart. For a 1-second window on a 1920-pixel wide screen, drawing every single one of the 1000 points is unnecessary; the system can render a "min/max" line for each pixel column (visual decimation) to maintain visual accuracy without choking the render thread.
4. Comprehensive Feature Implementation Plan
4.1 Telemetry and Data Logging
The core function of the CT-3 is logging voltage (V), current (A), and power (W). The software must replicate the "Basic" panel of the original application.4
Real-time Numerical Display: Large, high-contrast 7-segment style digits for V, A, and W. The color coding should match standard electronics conventions (e.g., Yellow for Voltage, Cyan for Current).
Trend Graphing: A scrolling "strip chart" showing the last T seconds of data. The user must be able to pause the scrolling to inspect historical data while logging continues in the background.
Triggered Recording: The software must verify the "Start Current" threshold. The logic is: if (Current > StartThreshold &&!IsRecording) StartLogging();. Similarly for the "Stop Current" and "Stop Delay" parameters, which prevent the logger from cutting out during momentary current dips.4
Data Persistence: To ensure data integrity, logs should be written to a local SQLite database or a CSV file stream in real-time. This prevents data loss if the application crashes during a long-duration test (e.g., battery capacity testing).
4.2 VBUS Ripple (Oscilloscope)
This feature differentiates the CT-3 from cheaper testers. The implementation requires specific UI controls mirroring a benchtop scope 4:
Timebase Control: A slider or knob to adjust the sample rate (0.1 Msps to 3.2 Msps).
Trigger Menu: Dropdowns for "Edge" (Rising/Falling) and "Mode" (Auto/Normal/Single). A numeric input for "Trigger Level" (Voltage).
Cursor Measurements: The user must be able to click and drag "cursors" on the graph to measure the delta-voltage (Vpp) and delta-time (Frequency) of the ripple waveform.
4.3 Power Delivery (PD) Listener
The PD Listener requires decoding the USB PD BMC (Biphase Mark Code) messages passed by the device.4 The CT-3 hardware handles the physical layer decoding and sends parsed packets to the PC.
Packet List: A DataGrid view showing a log of all PD packets. Columns: Timestamp, Type (Control/Data), Message (e.g., Source\_Capabilities), and Object Count.
Detail View: When a packet is selected, a side panel must decode the payload. For a Source\_Capabilities message, this implies parsing the 32-bit PDOs (Power Data Objects) to extract Voltage, Max Current, and Type (Fixed, Battery, PPS, AVS).
Integration: This module requires strict adherence to the USB PD 3.1 specification to correctly map the hex values to human-readable text (e.g., decoding the "Augmented Power Data Object" bits for PPS voltages).
4.4 Lua Scripting Environment
The CT-3 supports on-device Lua scripting for automated testing.2 The PC software acts as the IDE for this feature.
Editor: A syntax-highlighting text editor (using AvaloniaEdit) supporting the Lua language.
API Autocomplete: The editor should be pre-loaded with the Shizuku API definitions (meter.read(), pd.request()) to provide IntelliSense-like suggestions.
Console Output: A terminal window that displays text printed by the print() function in the Lua script running on the meter. This requires the "TCP Bridge" to be active, forwarding the standard output from the device to the PC UI.
4.5 Fast Charge Trigger
This feature allows the user to force the power adapter into a specific voltage mode.
Protocol Selection: A grid of buttons for PD, QC2.0, QC3.0, FCP, SCP, AFC, etc.
Safety Interlock: A "Master Arm" switch or a confirmation dialog before applying high voltages (e.g., 20V), as this can damage connected loads that are not 20V tolerant.
PPS Control: For Programmable Power Supply (PPS) mode, the UI must provide a slider to adjust voltage in 20mV increments and current in 50mA increments, sending the corresponding PD RDO (Request Data Object) updates to the charger in real-time.2
5. User Interface Design and Experience (UX)
The transition from the legacy Windows Forms app to Avalonia allows for a complete reimplementation of the User Experience. The guiding philosophy is "Cognitive Clarity"—providing maximum data density without visual clutter.
5.1 The Dashboard Layout
The main window is divided into three zones:
Sidebar (Navigation): A vertical strip on the left containing icons for the major modules: "Meter", "Chart", "Ripple", "PD Listen", "Scripts", and "Settings".
Main Stage (Content): The central area where the active module is rendered.
Status Bar (System State): A thin strip at the bottom displaying connection status ("Connected - COM3"), current sample rate ("1.0 kS/s"), and device temperature ("35.2°C").
5.2 Dark Mode and Theming
Engineering environments are often dimly lit to reduce screen glare. The application will default to a high-contrast Dark Mode.22
Backgrounds: Deep charcoal (#1E1E1E) rather than pure black to reduce eye strain.
Data Colors: Bright, neon-adjacent colors for data lines to ensure visibility against the dark background.
Voltage: #FFD700 (Gold)
Current: #00BFFF (Deep Sky Blue)
Power: #FF4500 (Orange Red)
Typography: A monospaced font (e.g., JetBrains Mono or Consolas) for all numerical data to ensure digit alignment, preventing the UI from "jittering" as values change rapidly.
5.3 Responsiveness and Accessibility
The UI must use relative sizing (Grids and DockPanels) rather than absolute positioning. This ensures that the chart expands to fill the available space whether the app is running on a 13-inch laptop or a 27-inch 4K monitor. Touch targets (buttons) should be sized appropriately (min 44x44 pixels) to support tablet usage, as engineers often use touch-enabled portable monitors in the field.
6. Cross-Platform Build & Deployment Strategy
This section explicitly details the build process for a Linux-hosted development environment targeting Windows x64, Windows on Arm, and Linux x64. The.NET 8 SDK allows for seamless cross-compilation without requiring a Windows host machine.
6.1 Build Environment Requirements
OS: Linux (Ubuntu 22.04 LTS or 24.04 LTS recommended).
SDK:.NET 8.0 SDK (sudo apt-get install dotnet-sdk-8.0).
Git: For version control.
Dependencies: Avalonia may require Skia dependencies on the build host for previewing, though strictly for compilation, the SDK is sufficient.
Install: libc6-dev, libskia, fontconfig.
6.2 Deployment Strategy: Self-Contained Single File
To emulate the ease of the original "System Box" portable executable, we will use the Self-Contained deployment mode. This bundles the.NET Runtime and all dependencies into a single binary, ensuring the user does not need to install.NET 8 manually.
Advantages:
Zero Dependency: The end user (on Windows or Linux) just runs the executable.
Version Control: You control the exact runtime version, preventing "it works on my machine" issues caused by mismatched installed runtimes.
Size: While larger (approx. 40-60MB), trimming is enabled to remove unused code.
6.3 Build Commands (Run from Linux Terminal)
Execute these commands from the solution root directory.
Target 1: Linux x86\_64 (Native)
This builds the binary for the machine you are developing on (and other standard Linux desktops).

Bash


dotnet publish -c Release \\
  -r linux-x64 \\
  --self-contained true \\
  -p:PublishSingleFile=true \\
  -p:IncludeNativeLibrariesForSelfExtract=true \\
  -p:PublishTrimmed=false \\
  -o./dist/linux-x64


Note: We disable full trimming (PublishTrimmed=false) initially to avoid aggressive removal of reflection-heavy libraries like serial port handlers or JSON parsers. It can be enabled later with careful configuration.
Target 2: Windows 11 x86\_64 (Standard Desktop)
The Linux SDK can cross-compile this natively.

Bash


dotnet publish -c Release \\
  -r win-x64 \\
  --self-contained true \\
  -p:PublishSingleFile=true \\
  -p:IncludeNativeLibrariesForSelfExtract=true \\
  -p:PublishReadyToRun=true \\
  -o./dist/win-x64


Target 3: Windows on Arm (Win11 Arm64 / Surface Pro X / Virtual Machines)
.NET 8 has first-class support for win-arm64. Avalonia UI also supports this target natively.

Bash


dotnet publish -c Release \\
  -r win-arm64 \\
  --self-contained true \\
  -p:PublishSingleFile=true \\
  -p:IncludeNativeLibrariesForSelfExtract=true \\
  -o./dist/win-arm64


6.4 Automated Build Pipeline (CI/CD)
To ensure stability, a GitHub Actions workflow file (.github/workflows/build.yml) is recommended. This will automatically generate these binaries on every commit.

YAML


name: Build Cross-Platform Binaries
on: \[push, pull\_request\]
jobs:
  build:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3
    - name: Setup.NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.0.x
    - name: Publish Linux
      run: dotnet publish -c Release -r linux-x64 --self-contained true -o release/linux-x64
    - name: Publish Windows x64
      run: dotnet publish -c Release -r win-x64 --self-contained true -o release/win-x64
    - name: Publish Windows Arm64
      run: dotnet publish -c Release -r win-arm64 --self-contained true -o release/win-arm64
    - name: Upload Artifacts
      uses: actions/upload-artifact@v3
      with:
        name: shizuku-suite-binaries
        path: release/


7. AI-Assisted Development Workflow
The implementation of this complex system will be accelerated using a triad of AI tools: Cursor Pro for codebase management and context-aware editing, Claude 3.5 Sonnet for high-level architectural reasoning and protocol decoding, and Codium for unit test generation.
7.1 Cursor Pro Environment Setup
To maximize the efficacy of Cursor Pro, a .cursorrules file must be established at the repository root. This file primes the AI with the project's specific constraints, ensuring that generated code aligns with the architecture defined in Section 3 and the build requirements in Section 6.
System Prompt Configuration (.cursorrules):
#.cursorrules - Shizuku Cross-Platform Control Suite
Project Context
You are a Senior Systems Architect and.NET Developer building a replacement control software for the Shizuku CT-3 USB Multimeter.
Framework:.NET 8 (C#)
UI Framework: Avalonia UI (Latest Stable)
Pattern: MVVM (CommunityToolkit.Mvvm)
Charting: ScottPlot 5.0
Development OS: Linux (Ubuntu)
Target OS: Windows (x64/Arm64), Linux (x64)
Engineering Standards
Cross-Platform IO: NEVER use hardcoded file paths (e.g., "C:"). Always use Path.Combine() and Environment.SpecialFolder.
Serial Port Safety: When enumerating ports, filter for standard Linux paths (/dev/ttyUSB\*, /dev/ttyACM\*) AND Windows paths (COM\*).
Concurrency: All high-frequency data ingestion must happen on a background thread (Task.Run). Only final UI updates are marshaled to the UI thread (Dispatcher.UIThread).
Safety First: Never generate code that writes to a Serial Port without a try/catch block and a CancellationToken. Hardware I/O is inherently unstable.
Build Constraints
The project is built on Linux.
Do not use Windows-specific APIs (like Registry) unless wrapped in OperatingSystem.IsWindows() checks.
7.2 Prompt Engineering Strategy
The interaction with the AI tools is divided into phases, each requiring distinct prompt subtypes.
Phase 1: Protocol Reverse Engineering (Claude 3.5 Sonnet)
In this phase, the goal is to decipher the binary stream. The prompt must be analytical and data-centric.
Prompt Subtype: Hex Pattern Analysis
Personalization: "You are an expert protocol engineer specializing in USB analysis."
Instruction: "I will provide you with two hex dumps. Dump A is captured while the device was Idle. Dump B is captured immediately after I clicked 'Start Logging'. Compare the two streams. Identify the command packet that initiated the logging. Look for a preamble (likely 0xAA or 0xFE), a length byte, and a checksum. Propose a C# struct that represents this command."
Input: \`\`
Phase 2: UI Boilerplate Generation (Cursor Composer)
This phase focuses on generating the verbose XAML required for the dashboard.
Prompt Subtype: XAML Visual Design
Instruction: "Act as an Avalonia UI expert. Create a UserControl named MeasurementCard.axaml. It needs to display a Label, a Value, and a Unit.
The Value must be a large TextBlock (FontSize=32) bound to a Value property.
The Unit must be smaller (FontSize=16) and aligned to the baseline of the Value.
Wrap the control in a Border with CornerRadius=8, a generic background color resource SolidColorBrush x:Key='SurfaceBrush', and a subtle drop shadow.
Provide the C# code-behind with the necessary AvaloniaProperty definitions for data binding."
Phase 3: Unit Testing and Validation (Codium)
This phase ensures the parser logic is robust against malformed data.
Prompt Subtype: Edge Case Generation
Instruction: "You are a QA Automation Engineer. I have a class ProtocolParser with a method ParsePacket(byte data).
Generate a generic NUnit test suite.
Include test cases for:
A perfectly valid packet.
A packet with an invalid CRC checksum (method should throw InvalidChecksumException).
A packet that is shorter than the expected length.
A packet with random noise bytes appended to the end.
Use FluentAssertions for the assertions."
Phase 4: Build & Deployment (Claude/Cursor)
This phase helps debug cross-platform build issues.
Prompt Subtype: Build Engineer
Personalization: "You are a DevOps engineer specializing in.NET Core cross-compilation."
Instruction: "I am building this Avalonia application on Linux for a win-arm64 target. I am getting an error regarding libSkiaSharp. Explain how to reference the correct NuGet packages to ensure the native assets for Windows Arm64 are included in the publish directory, even though the build host is Linux x64."
8. Implementation Roadmap
Phase I: Foundation (Weeks 1-2)
Initialize the.NET 8 solution with Avalonia and CommunityToolkit.Mvvm.
Configure the .csproj for multi-RID publication (win-x64, win-arm64, linux-x64).
Implement the SerialService using a cross-platform library like SerialPortStream.
Phase II: The Core Logger (Weeks 3-4)
Implement the TelemetryParser based on reverse-engineered specs.
Create the LoggerViewModel and the circular buffer logic.
Integrate ScottPlot 5.0 and bind it to the data stream.
Milestone: A working "Digital Multimeter" on the desktop.
Phase III: Advanced Oscilloscope (Weeks 5-7)
Implement the "Bulk Read" state machine for the Ripple feature.
Create the specialized RippleScopeView with trigger controls.
Optimize rendering pipeline to handle the data bursts without UI freeze.
Phase IV: Protocol & Automation (Weeks 8-10)
Implement the PD Listener packet decoding.
Integrate the AvaloniaEdit control for Lua scripting.
Implement the TCP Bridge server to mimic the "Shizuku Toolbox" behavior.
Phase V: Polish & Release (Week 11)
Finalize Dark Mode themes and iconography.
Conduct long-duration stress tests (24+ hours).
Run the manual build scripts defined in Section 6.3 to generate the final release artifacts.
9. Conclusion
The "Shizuku System Box" replacement project is a necessary evolution for the CT-3 ecosystem. By moving from a closed, Windows-centric architecture to an open, cross-platform.NET 8/Avalonia foundation, we unlock the full potential of the hardware for the modern engineering workforce. The strategy detailed above—leveraging the precision of hardware reverse engineering combined with the accelerating power of AI coding assistants—provides a clear path to delivering a professional-grade instrumentation tool that meets and exceeds the original manufacturer's specifications. The result will be a software suite that is as robust, versatile, and precise as the multimeter it controls.
Appendix: Key Research References
4: Original Manuals defining feature set (Ripple, Logging).
10: Protocol details regarding Serial and TCP bridge methods.
2: DingoCharge and Lua API documentation.
14: Avalonia UI capabilities for cross-platform scientific dashboards.
5: Prior art on reverse-engineering the CT-3 wire protocol.
3: Technical specifications regarding sampling rates (3.2Msps Ripple, 1000sps Logging).
9: Lua API interaction and TCP module details.
: Cross-platform compilation for Windows Arm64 and Linux using.NET 8.
Works cited
USB power meter/tester thread - YZXStudio, Power-Z, RDTech and more. - Page 19, accessed February 7, 2026, https://budgetlightforum.com/t/usb-power-meter-tester-thread-yzxstudio-power-z-rdtech-and-more/37662?page=19
ginbot86/DingoCharge-Shizuku: USB PD/PPS direct ... - GitHub, accessed February 7, 2026, https://github.com/ginbot86/DingoCharge-Shizuku
Shizuku / AVHzY CT-3 / Power-Z KT002 / Atorch UT18 USB Tester and SM-LD-00 Module Review, accessed February 7, 2026, https://usbchargingblog.wordpress.com/2021/01/07/shizuku-avhzy-ct-3-power-z-kt002-atorch-ut18-usb-tester-and-sm-ld-00-module-review/
Shizuku USB Multimeter PC Software UI Manual V1.00.44, accessed February 7, 2026, https://supereyes.ru/img/instructions/userguide\_pc\_software.pdf
Reverse engineering the wire protocol for the AVHzy CT-3 Power meter - Peter Membrey, accessed February 7, 2026, https://the.engineer/blog/2023/11/26/reverse-engineering-the-wire-protocol-for-the-avhzy-ct-3-power-meter/
Best Affordable USB Analyzer QC/PD AVHzY CT-3 Shizuku | Voltlog #407 - YouTube, accessed February 7, 2026, https://www.youtube.com/watch?v=GJwqnkUd8XU
Shizuku USB Multimeter - YK-LAB, accessed February 7, 2026, https://yk-lab.org:666/shizuku/manual/software/manual-pc-en-us/content.html
Shizuku CT-3 USB Multimeter PC Software User Manual - device.report, accessed February 7, 2026, https://device.report/manual/9796122
AVHzY CT-3 USB Power Meter Tester Review - JunctionByte, accessed February 7, 2026, https://junctionbyte.com/avhzy-ct-3-usb-power-meter-tester-review/
tcp module — lua API documentation - YK-LAB, accessed February 7, 2026, https://yk-lab.org:666/shizuku/lua-api/lua-api-en-us/Modules/tcp.html
Reverse Engineering USB Protocol - GitHub, accessed February 7, 2026, https://github.com/openrazer/openrazer/wiki/Reverse-Engineering-USB-Protocol
Best Serial Data Logger Software for Sensor Monitoring? : r/embedded - Reddit, accessed February 7, 2026, https://www.reddit.com/r/embedded/comments/1gersgf/best\_serial\_data\_logger\_software\_for\_sensor/
USB: what are the advantages (or disadvantages) or using HID over serial-over-USB?, accessed February 7, 2026, https://electronics.stackexchange.com/questions/21383/usb-what-are-the-advantages-or-disadvantages-or-using-hid-over-serial-over-us
AvaloniaUI/Avalonia: Develop Desktop, Embedded, Mobile and WebAssembly apps with C# and XAML. The most popular .NET UI client technology - GitHub, accessed February 7, 2026, https://github.com/AvaloniaUI/Avalonia
Is AvaloniaUI good option for multiplatform GUI in 2024? : r/dotnet - Reddit, accessed February 7, 2026, https://www.reddit.com/r/dotnet/comments/1al38a1/is\_avaloniaui\_good\_option\_for\_multiplatform\_gui/
Is Avalonia UI a Viable Alternative to Electron.js for Enterprise Applications?, accessed February 7, 2026, https://hicronsoftware.com/blog/avalonia-ui-for-enterprise-applications/
Avalonia: Cross-Platform .NET UI Framework, accessed February 7, 2026, https://avaloniaui.net/platforms
SciChart Avalonia XPF: High-Performance Charting for Windows & Linux, accessed February 7, 2026, https://www.scichart.com/introducing-avalonia-xpf-2/
ScottPlot - Interactive Plotting Library for .NET, accessed February 7, 2026, https://scottplot.net/
Top Open-Source .NET Charting Libraries for Data Visualization - Ecweb.com, accessed February 7, 2026, https://ecweb.ecer.com/topic/cn/detail-34379-top\_opensource\_net\_charting\_libraries\_for\_data\_visualization.html
Help Selecting a High-Performance Plotting Library for Real-Time Data : r/dotnet - Reddit, accessed February 7, 2026, https://www.reddit.com/r/dotnet/comments/1h2m0dw/help\_selecting\_a\_highperformance\_plotting\_library/
Dashboard Design UX Patterns Best Practices - Pencil & Paper, accessed February 7, 2026, https://www.pencilandpaper.io/articles/ux-pattern-analysis-data-dashboards
CT3/aserial - GitHub, accessed February 7, 2026, https://github.com/CT3/aserial