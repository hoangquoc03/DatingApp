using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Controls;
using DatingApp.Desktop.Views;

namespace DatingApp.Desktop.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private object _currentView;

    public MainViewModel(LoginViewModel loginViewModel)
    {
        // Start with LoginView
        CurrentView = loginViewModel;
    }
}
