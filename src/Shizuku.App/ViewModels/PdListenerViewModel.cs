using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Shizuku.App.ViewModels;

/// <summary>
/// View model for the USB PD Listener.
/// </summary>
public partial class PdListenerViewModel : ViewModelBase
{
    public override string DisplayName => "PD Listen";

    [ObservableProperty] private bool _isCapturing;
    [ObservableProperty] private PdPacketRow? _selectedPacket;

    public ObservableCollection<PdPacketRow> Packets { get; } = new();

    [RelayCommand]
    private void ToggleCapture()
    {
        IsCapturing = !IsCapturing;
    }

    [RelayCommand]
    private void ClearPackets()
    {
        Packets.Clear();
    }
}

/// <summary>Row model for the PD packet DataGrid.</summary>
public partial class PdPacketRow : ObservableObject
{
    [ObservableProperty] private string _timestamp = string.Empty;
    [ObservableProperty] private string _type = string.Empty;
    [ObservableProperty] private string _message = string.Empty;
    [ObservableProperty] private int _objectCount;
    [ObservableProperty] private string _details = string.Empty;
}
