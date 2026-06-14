using System.Windows.Controls;

namespace DatingApp.Desktop.Views;

public partial class LoginView : System.Windows.Controls.UserControl
{
    public LoginView()
    {
        InitializeComponent();
    }

    private void PasswordBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is ViewModels.LoginViewModel vm)
        {
            vm.Password = ((PasswordBox)sender).Password;
        }
    }

    private void RegisterPasswordBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is ViewModels.LoginViewModel vm)
        {
            vm.RegisterPassword = ((PasswordBox)sender).Password;
        }
    }

    private void ResetNewPasswordBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is ViewModels.LoginViewModel vm)
        {
            vm.ResetNewPassword = ((PasswordBox)sender).Password;
        }
    }

    private void ResetConfirmPasswordBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is ViewModels.LoginViewModel vm)
        {
            vm.ResetConfirmPassword = ((PasswordBox)sender).Password;
        }
    }
}
