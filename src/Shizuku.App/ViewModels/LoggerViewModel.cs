using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Shizuku.App.ViewModels;

/// <summary>
/// View model for the telemetry logger with start/stop and CSV export controls.
/// </summary>
public partial class LoggerViewModel : ViewModelBase
{
    public override string DisplayName => "Chart";

    [ObservableProperty] private bool _isRecording;
    [ObservableProperty] private string _logFilePath = string.Empty;
    [ObservableProperty] private long _rowsWritten;
    [ObservableProperty] private double _startCurrentThreshold = 0.01;
    [ObservableProperty] private double _stopCurrentThreshold = 0.005;
    [ObservableProperty] private int _stopDelaySeconds = 5;

    [RelayCommand]
    private void ToggleRecording()
    {
        IsRecording = !IsRecording;
        // Actual start/stop logic will be wired to the Services layer.
    }
}
