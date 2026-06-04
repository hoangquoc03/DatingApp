using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DatingApp.Desktop.Services;
using System.Windows;
using System;

namespace DatingApp.Desktop.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly AuthService _authService;
    private bool _isNewUserSession = false;

    [ObservableProperty]
    private string _email = "admin@gmail.com"; // Default for testing

    [ObservableProperty]
    private string _password = "123456";

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _errorMessage = "";

    [ObservableProperty]
    private bool _isLoginPanelVisible = true;

    [ObservableProperty]
    private bool _isRegisterStep1Visible = false;

    [ObservableProperty]
    private bool _isRegisterStep2Visible = false;

    // Register Properties
    [ObservableProperty]
    private string _registerEmail = "";

    [ObservableProperty]
    private string _registerPassword = "";

    [ObservableProperty]
    private string _registerFullName = "";

    [ObservableProperty]
    private DateTime _registerDOB = DateTime.Now.AddYears(-18);

    [ObservableProperty]
    private int _registerGender = 1; // 0: Nam, 1: Nữ, 2: Khác

    public LoginViewModel(AuthService authService)
    {
        _authService = authService;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        ErrorMessage = "";
        IsBusy = true;
        try
        {
            var success = await _authService.LoginAsync(Email, Password);
            if (success)
            {
                var app = (App)System.Windows.Application.Current;

                // Admin → AdminViewModel
                // User chưa onboard & mới đăng ký → OnboardingViewModel
                // Các trường hợp khác (kể cả user cũ chưa onboard) → DashboardViewModel
                object? nextViewModel;
                if (_authService.CurrentUser?.IsAdmin == true)
                {
                    nextViewModel = app.Services.GetService(typeof(AdminViewModel));
                }
                else if (_authService.CurrentUser?.IsOnboarded == false && _isNewUserSession)
                {
                    nextViewModel = app.Services.GetService(typeof(OnboardingViewModel));
                }
                else
                {
                    nextViewModel = app.Services.GetService(typeof(DashboardViewModel));
                }

                _isNewUserSession = false; // Reset cờ

                CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(
                    new DatingApp.Desktop.Messages.NavigationMessage(nextViewModel!)
                );
            }
            else
            {
                ErrorMessage = "Đăng nhập thất bại. Kiểm tra lại email/mật khẩu.";
            }
        }
        catch (System.Exception ex)
        {
            ErrorMessage = "Lỗi kết nối: " + ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void ShowRegisterStep1()
    {
        IsLoginPanelVisible = false;
        IsRegisterStep1Visible = true;
        IsRegisterStep2Visible = false;
        ErrorMessage = "";
    }

    [RelayCommand]
    private void ShowRegisterStep2()
    {
        if (string.IsNullOrWhiteSpace(RegisterEmail) || string.IsNullOrWhiteSpace(RegisterPassword))
        {
            ErrorMessage = "Vui lòng nhập Email và Mật khẩu!";
            return;
        }
        IsLoginPanelVisible = false;
        IsRegisterStep1Visible = false;
        IsRegisterStep2Visible = true;
        ErrorMessage = "";
    }

    [RelayCommand]
    private void BackToLogin()
    {
        IsLoginPanelVisible = true;
        IsRegisterStep1Visible = false;
        IsRegisterStep2Visible = false;
        ErrorMessage = "";
    }

    [RelayCommand]
    private async Task CompleteRegisterAsync()
    {
        if (string.IsNullOrWhiteSpace(RegisterFullName))
        {
            ErrorMessage = "Vui lòng nhập Họ Tên!";
            return;
        }

        ErrorMessage = "";
        IsBusy = true;
        try
        {
            var dto = new DatingApp.Desktop.Models.RegisterDto
            {
                Email = RegisterEmail,
                Password = RegisterPassword,
                FullName = RegisterFullName,
                DateOfBirth = RegisterDOB,
                Gender = RegisterGender
            };

            var success = await _authService.RegisterAsync(dto);
            if (success)
            {
                System.Windows.MessageBox.Show("Đăng ký thành công! Hãy đăng nhập.", "Thông báo", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                _isNewUserSession = true;
                BackToLogin();
                Email = RegisterEmail; // Autofill email for user
            }
            else
            {
                ErrorMessage = "Đăng ký thất bại. Email có thể đã tồn tại.";
            }
        }
        catch (System.Exception ex)
        {
            ErrorMessage = "Lỗi kết nối: " + ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
