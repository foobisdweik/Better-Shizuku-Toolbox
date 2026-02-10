using CommunityToolkit.Mvvm.ComponentModel;

namespace Shizuku.App.ViewModels;

/// <summary>
/// Dashboard view model showing real-time voltage, current, power, and statistics.
/// </summary>
public partial class DashboardViewModel : ViewModelBase
{
    public override string DisplayName => "Meter";

    [ObservableProperty] private double _voltageV;
    [ObservableProperty] private double _currentA;
    [ObservableProperty] private double _powerW;
    [ObservableProperty] private double _temperatureC;

    [ObservableProperty] private double _minVoltageV;
    [ObservableProperty] private double _maxVoltageV;
    [ObservableProperty] private double _avgVoltageV;

    [ObservableProperty] private double _minCurrentA;
    [ObservableProperty] private double _maxCurrentA;
    [ObservableProperty] private double _avgCurrentA;

    [ObservableProperty] private double _accumulatedMah;
    [ObservableProperty] private double _accumulatedMwh;

    [ObservableProperty] private long _sampleCount;
    [ObservableProperty] private string _elapsedText = "00:00:00";
}
