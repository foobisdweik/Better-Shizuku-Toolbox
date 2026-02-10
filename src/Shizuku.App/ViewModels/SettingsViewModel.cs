using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Shizuku.App.ViewModels;

/// <summary>
/// View model for application settings and device connection.
/// </summary>
public partial class SettingsViewModel : ViewModelBase
{
    public override string DisplayName => "Settings";

    [ObservableProperty] private string _selectedPort = string.Empty;
    [ObservableProperty] private int _baudRate = 115200;
    [ObservableProperty] private bool _isConnected;

    public ObservableCollection<string> AvailablePorts { get; } = new();

    [RelayCommand]
    private void RefreshPorts()
    {
        AvailablePorts.Clear();
        foreach (var port in Shizuku.Hardware.Discovery.DeviceDiscovery.GetCandidatePorts())
        {
            AvailablePorts.Add(port);
        }

        if (AvailablePorts.Count > 0 && string.IsNullOrEmpty(SelectedPort))
        {
            SelectedPort = AvailablePorts[0];
        }
    }

    [RelayCommand]
    private void ToggleConnection()
    {
        IsConnected = !IsConnected;
        // Will be wired to SessionManager.ConnectAsync / DisconnectAsync.
    }
}
