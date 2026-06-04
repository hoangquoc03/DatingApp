using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DatingApp.Desktop.Services;
using System.Windows;

namespace DatingApp.Desktop.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly AuthService _authService;

    [ObservableProperty]
    private string _email = "admin@gmail.com"; // Default for testing

    [ObservableProperty]
    private string _password = "123456";

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public LoginViewModel(AuthService authService)
    {
        _authService = authService;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Vui lòng nhập Email và Mật khẩu.";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var success = await _authService.LoginAsync(Email, Password);
            if (success)
            {
                // TODO: Navigate to MainDashboard
                MessageBox.Show("Đăng nhập thành công!", "Thông báo");
            }
            else
            {
                ErrorMessage = "Email hoặc Mật khẩu không chính xác.";
            }
        }
        catch (System.Exception ex)
        {
            ErrorMessage = "Lỗi kết nối máy chủ: " + ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
