using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using DatingApp.Desktop.Messages;
using DatingApp.Desktop.Views;

namespace DatingApp.Desktop.ViewModels;

public partial class MainViewModel : ObservableObject, IRecipient<NavigationMessage>
{
    [ObservableProperty]
    private object _currentView;

    public MainViewModel(LoginViewModel loginViewModel)
    {
        // Start with LoginView
        CurrentView = loginViewModel;
        WeakReferenceMessenger.Default.Register(this);
    }

    public void Receive(NavigationMessage message)
    {
        CurrentView = message.Value;
    }
}
