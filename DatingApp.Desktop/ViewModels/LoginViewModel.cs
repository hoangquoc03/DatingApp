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
    private string _tempPassword = "";

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

    [ObservableProperty]
    private bool _isForgotPasswordVisible = false;

    [ObservableProperty]
    private bool _isResetPasswordVisible = false;

    [ObservableProperty]
    private bool _isOtpVerificationVisible = false;

    // Forgot / Reset / OTP Properties
    [ObservableProperty]
    private string _forgotEmail = "";

    [ObservableProperty]
    private string _resetToken = "";

    [ObservableProperty]
    private string _resetNewPassword = "";

    [ObservableProperty]
    private string _resetConfirmPassword = "";

    [ObservableProperty]
    private string _otpVerificationCode = "";

    [ObservableProperty]
    private string _otpTargetEmail = "";

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
                else if (_authService.CurrentUser?.IsOnboarded == false)
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
            if (ex.Message == "EMAIL_NOT_VERIFIED")
            {
                // Người dùng chưa xác thực email -> Chuyển sang màn hình xác minh OTP
                _tempPassword = Password;
                OtpTargetEmail = Email;
                OtpVerificationCode = "";
                ErrorMessage = "";
                IsLoginPanelVisible = false;
                IsRegisterStep1Visible = false;
                IsRegisterStep2Visible = false;
                IsForgotPasswordVisible = false;
                IsResetPasswordVisible = false;
                IsOtpVerificationVisible = true;
                System.Windows.MessageBox.Show("Email của bạn chưa được xác thực. Chúng tôi đã gửi một mã OTP mới đến email của bạn.", "Xác thực Email", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            }
            else
            {
                ErrorMessage = "Lỗi kết nối: " + ex.Message;
            }
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
        IsForgotPasswordVisible = false;
        IsResetPasswordVisible = false;
        IsOtpVerificationVisible = false;
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
        IsForgotPasswordVisible = false;
        IsResetPasswordVisible = false;
        IsOtpVerificationVisible = false;
        ErrorMessage = "";
    }

    [RelayCommand]
    private void BackToLogin()
    {
        IsLoginPanelVisible = true;
        IsRegisterStep1Visible = false;
        IsRegisterStep2Visible = false;
        IsForgotPasswordVisible = false;
        IsResetPasswordVisible = false;
        IsOtpVerificationVisible = false;
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
                System.Windows.MessageBox.Show("Đăng ký thành công! Hãy nhập mã xác thực OTP đã được gửi tới email của bạn.", "Thông báo", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                _isNewUserSession = true;
                _tempPassword = RegisterPassword;
                
                // Chuyển sang OTP panel
                OtpTargetEmail = RegisterEmail;
                OtpVerificationCode = "";
                
                IsLoginPanelVisible = false;
                IsRegisterStep1Visible = false;
                IsRegisterStep2Visible = false;
                IsForgotPasswordVisible = false;
                IsResetPasswordVisible = false;
                IsOtpVerificationVisible = true;
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

    [RelayCommand]
    private void ShowForgotPassword()
    {
        IsLoginPanelVisible = false;
        IsRegisterStep1Visible = false;
        IsRegisterStep2Visible = false;
        IsForgotPasswordVisible = true;
        IsResetPasswordVisible = false;
        IsOtpVerificationVisible = false;
        ForgotEmail = Email; // Autofill email if they had typed one
        ErrorMessage = "";
    }

    [RelayCommand]
    private async Task SendResetLinkAsync()
    {
        if (string.IsNullOrWhiteSpace(ForgotEmail))
        {
            ErrorMessage = "Vui lòng nhập Email!";
            return;
        }

        ErrorMessage = "";
        IsBusy = true;
        try
        {
            var success = await _authService.ForgotPasswordAsync(ForgotEmail);
            if (success)
            {
                System.Windows.MessageBox.Show("Yêu cầu đặt lại mật khẩu đã được xử lý. Vui lòng lấy mã phục hồi từ email/log hệ thống để tiếp tục.", "Thành công");
                // Chuyển sang màn Đặt lại mật khẩu
                ResetToken = "";
                ResetNewPassword = "";
                ResetConfirmPassword = "";
                IsLoginPanelVisible = false;
                IsRegisterStep1Visible = false;
                IsRegisterStep2Visible = false;
                IsForgotPasswordVisible = false;
                IsResetPasswordVisible = true;
                IsOtpVerificationVisible = false;
            }
            else
            {
                ErrorMessage = "Không thể gửi yêu cầu đặt lại mật khẩu.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "Lỗi: " + ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ResetPasswordAsync()
    {
        if (string.IsNullOrWhiteSpace(ResetToken))
        {
            ErrorMessage = "Vui lòng nhập Mã phục hồi (Token)!";
            return;
        }
        if (string.IsNullOrWhiteSpace(ResetNewPassword))
        {
            ErrorMessage = "Vui lòng nhập mật khẩu mới!";
            return;
        }
        if (ResetNewPassword != ResetConfirmPassword)
        {
            ErrorMessage = "Mật khẩu xác nhận không khớp!";
            return;
        }

        ErrorMessage = "";
        IsBusy = true;
        try
        {
            var success = await _authService.ResetPasswordAsync(ResetToken, ResetNewPassword);
            if (success)
            {
                System.Windows.MessageBox.Show("Đặt lại mật khẩu thành công! Vui lòng đăng nhập bằng mật khẩu mới.", "Thành công");
                Email = ForgotEmail; // Autofill the email
                BackToLogin();
            }
            else
            {
                ErrorMessage = "Mã phục hồi không hợp lệ hoặc đã hết hạn.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "Lỗi: " + ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task VerifyOtpAsync()
    {
        if (string.IsNullOrWhiteSpace(OtpVerificationCode) || OtpVerificationCode.Length != 6)
        {
            ErrorMessage = "Vui lòng nhập mã OTP gồm 6 chữ số!";
            return;
        }

        ErrorMessage = "";
        IsBusy = true;
        try
        {
            var success = await _authService.VerifyEmailAsync(OtpTargetEmail, OtpVerificationCode);
            if (success)
            {
                // Auto login if tempPassword is set
                if (!string.IsNullOrEmpty(_tempPassword))
                {
                    var loginSuccess = await _authService.LoginAsync(OtpTargetEmail, _tempPassword);
                    _tempPassword = ""; // clear
                    if (loginSuccess)
                    {
                        var app = (App)System.Windows.Application.Current;
                        object? nextViewModel;
                        if (_authService.CurrentUser?.IsAdmin == true)
                        {
                            nextViewModel = app.Services.GetService(typeof(AdminViewModel));
                        }
                        else if (_authService.CurrentUser?.IsOnboarded == false)
                        {
                            nextViewModel = app.Services.GetService(typeof(OnboardingViewModel));
                        }
                        else
                        {
                            nextViewModel = app.Services.GetService(typeof(DashboardViewModel));
                        }

                        _isNewUserSession = false;

                        CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(
                            new DatingApp.Desktop.Messages.NavigationMessage(nextViewModel!)
                        );
                        return;
                    }
                }

                System.Windows.MessageBox.Show("Xác minh Email thành công! Bạn có thể đăng nhập ngay bây giờ.", "Thành công");
                Email = OtpTargetEmail;
                BackToLogin();
            }
            else
            {
                ErrorMessage = "Mã OTP không chính xác hoặc đã hết hạn!";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "Lỗi: " + ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ResendOtpAsync()
    {
        ErrorMessage = "";
        IsBusy = true;
        try
        {
            var pwd = !string.IsNullOrEmpty(Password) ? Password : RegisterPassword;
            if (string.IsNullOrEmpty(pwd))
            {
                ErrorMessage = "Không có mật khẩu trong phiên làm việc để gửi lại OTP. Vui lòng quay lại màn đăng nhập.";
                return;
            }

            try
            {
                await _authService.LoginAsync(OtpTargetEmail, pwd);
                ErrorMessage = "Đã gửi lại mã OTP mới. Vui lòng kiểm tra email/log!";
            }
            catch (Exception ex) when (ex.Message == "EMAIL_NOT_VERIFIED")
            {
                ErrorMessage = "Đã gửi lại mã OTP mới. Vui lòng kiểm tra email/log!";
            }
            catch (Exception ex)
            {
                ErrorMessage = "Lỗi gửi lại mã: " + ex.Message;
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}
