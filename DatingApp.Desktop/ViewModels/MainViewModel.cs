using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using DatingApp.Desktop.Messages;
using DatingApp.Desktop.Views;
using DatingApp.Desktop.Services;
using System;
using System.Threading.Tasks;

namespace DatingApp.Desktop.ViewModels;

public partial class MainViewModel : ObservableObject, IRecipient<NavigationMessage>
{
    [ObservableProperty]
    private object _currentView;

    public MainViewModel(LoginViewModel loginViewModel, AuthService authService, IServiceProvider serviceProvider)
    {
        // Start with LoginView
        CurrentView = loginViewModel;
        WeakReferenceMessenger.Default.Register(this);

        _ = TryAutoLoginAsync(authService, serviceProvider);
    }

    private async Task TryAutoLoginAsync(AuthService authService, IServiceProvider serviceProvider)
    {
        var success = await authService.TryAutoLoginAsync();
        if (success)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                object? nextViewModel;
                if (authService.CurrentUser?.IsAdmin == true)
                {
                    nextViewModel = serviceProvider.GetService(typeof(AdminViewModel));
                }
                else if (authService.CurrentUser?.IsOnboarded == false)
                {
                    nextViewModel = serviceProvider.GetService(typeof(OnboardingViewModel));
                }
                else
                {
                    nextViewModel = serviceProvider.GetService(typeof(DashboardViewModel));
                }

                if (nextViewModel != null)
                {
                    CurrentView = nextViewModel;
                }
            });
        }
    }

    public void Receive(NavigationMessage message)
    {
        CurrentView = message.Value;
    }
}
