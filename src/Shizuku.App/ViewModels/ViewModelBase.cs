using CommunityToolkit.Mvvm.ComponentModel;

namespace Shizuku.App.ViewModels;

/// <summary>
/// Base class for all view models. Provides <see cref="ObservableObject"/> change notification.
/// </summary>
public abstract class ViewModelBase : ObservableObject
{
    /// <summary>Display name shown in the sidebar or tab header.</summary>
    public abstract string DisplayName { get; }
}
