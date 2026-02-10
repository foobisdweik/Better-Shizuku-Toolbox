using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Shizuku.App.ViewModels;

/// <summary>
/// View model for the VBUS Ripple (DSO) view.
/// </summary>
public partial class RippleScopeViewModel : ViewModelBase
{
    public override string DisplayName => "Ripple";

    [ObservableProperty] private int _sampleRateHz = 100_000;
    [ObservableProperty] private int _sampleDepth = 4096;
    [ObservableProperty] private double _triggerLevelV;
    [ObservableProperty] private bool _triggerRisingEdge = true;
    [ObservableProperty] private string _triggerMode = "Auto";
    [ObservableProperty] private string _captureState = "Idle";

    [RelayCommand]
    private void ArmCapture()
    {
        CaptureState = "Armed";
        // Will send configuration to device and wait for trigger.
    }

    [RelayCommand]
    private void ForceTrigger()
    {
        CaptureState = "Triggered";
        // Forces an immediate capture regardless of trigger condition.
    }
}
