using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Shizuku.App.ViewModels;

/// <summary>
/// View model for the Lua scripting editor.
/// </summary>
public partial class ScriptEditorViewModel : ViewModelBase
{
    public override string DisplayName => "Scripts";

    [ObservableProperty] private string _scriptText = "-- Shizuku Lua script\nprint('Hello from CT-3!')\n";
    [ObservableProperty] private string _consoleOutput = string.Empty;
    [ObservableProperty] private bool _isRunning;

    [RelayCommand]
    private void RunScript()
    {
        IsRunning = true;
        ConsoleOutput = "[Uploading and executing script...]\n";
        // Will upload to device via TCP bridge and stream console output.
    }

    [RelayCommand]
    private void StopScript()
    {
        IsRunning = false;
        ConsoleOutput += "[Script stopped.]\n";
    }

    [RelayCommand]
    private void ClearConsole()
    {
        ConsoleOutput = string.Empty;
    }
}
