using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Shizuku.App.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private ViewModelBase _currentView;

    [ObservableProperty]
    private string _connectionStatus = "Disconnected";

    [ObservableProperty]
    private string _sampleRateText = "--";

    [ObservableProperty]
    private string _temperatureText = "--";

    public ObservableCollection<ViewModelBase> Views { get; }

    public DashboardViewModel Dashboard { get; }
    public LoggerViewModel Logger { get; }
    public RippleScopeViewModel RippleScope { get; }
    public PdListenerViewModel PdListener { get; }
    public ScriptEditorViewModel ScriptEditor { get; }
    public SettingsViewModel Settings { get; }

    public MainWindowViewModel()
    {
        Dashboard = new DashboardViewModel();
        Logger = new LoggerViewModel();
        RippleScope = new RippleScopeViewModel();
        PdListener = new PdListenerViewModel();
        ScriptEditor = new ScriptEditorViewModel();
        Settings = new SettingsViewModel();

        Views = new ObservableCollection<ViewModelBase>
        {
            Dashboard,
            Logger,
            RippleScope,
            PdListener,
            ScriptEditor,
            Settings,
        };

        _currentView = Dashboard;
    }

    [RelayCommand]
    private void Navigate(ViewModelBase target)
    {
        CurrentView = target;
    }
}
