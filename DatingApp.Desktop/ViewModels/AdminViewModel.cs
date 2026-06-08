using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DatingApp.Desktop.Models;
using DatingApp.Desktop.Services;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.Generic;

namespace DatingApp.Desktop.ViewModels;

public partial class AdminViewModel : ObservableObject
{
    private readonly HttpClient _httpClient;

    [ObservableProperty]
    private ObservableCollection<UserDto> _users = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isUserTabSelected = true;

    [ObservableProperty]
    private bool _isReportTabSelected = false;

    [ObservableProperty]
    private ObservableCollection<ReportDto> _reports = new();

    [ObservableProperty]
    private int _totalUsers;

    [ObservableProperty]
    private int _activeUsers;

    [ObservableProperty]
    private int _totalMatches;

    [ObservableProperty]
    private int _totalReports;

    [ObservableProperty]
    private int _unresolvedReports;

    [ObservableProperty]
    private int _verifiedUsers;

    public AdminViewModel(IHttpClientFactory httpClientFactory, AuthService authService)
    {
        _httpClient = httpClientFactory.CreateClient("ApiClient");
        // Set token trực tiếp — đảm bảo luôn có Bearer token
        if (!string.IsNullOrEmpty(authService.CurrentToken))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", authService.CurrentToken);
        }
        _ = LoadUsersAsync();
    }

    [RelayCommand]
    private async Task LoadUsersAsync()
    {
        IsLoading = true;
        try
        {
            var users = await _httpClient.GetFromJsonAsync<List<UserDto>>("/api/Admin/users");
            if (users != null)
            {
                Users = new ObservableCollection<UserDto>(users);
            }

            try
            {
                var stats = await _httpClient.GetFromJsonAsync<System.Text.Json.JsonElement>("/api/Admin/stats");
                TotalUsers = stats.GetProperty("totalUsers").GetInt32();
                ActiveUsers = stats.GetProperty("activeUsers").GetInt32();
                TotalMatches = stats.GetProperty("totalMatches").GetInt32();
                TotalReports = stats.GetProperty("totalReports").GetInt32();
                UnresolvedReports = stats.GetProperty("unresolvedReports").GetInt32();
                VerifiedUsers = stats.GetProperty("verifiedUsers").GetInt32();
            }
            catch {}
        }
        catch (System.Exception ex)
        {
            MessageBox.Show("Lỗi lấy danh sách User: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ToggleUserActiveAsync(UserDto user)
    {
        if (user == null) return;

        try
        {
            var response = await _httpClient.PostAsync($"/api/Admin/users/{user.Id}/toggle-active", null);
            if (response.IsSuccessStatusCode)
            {
                // Refresh list
                await LoadUsersAsync();
                MessageBox.Show("Thao tác thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                MessageBox.Show("Lỗi: " + error, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (System.Exception ex)
        {
            MessageBox.Show("Lỗi kết nối: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task ToggleUserVerifyAsync(UserDto user)
    {
        if (user == null) return;

        try
        {
            var response = await _httpClient.PostAsync($"/api/Admin/users/{user.Id}/toggle-verify", null);
            if (response.IsSuccessStatusCode)
            {
                // Refresh list & stats
                await LoadUsersAsync();
                MessageBox.Show("Cập nhật trạng thái xác thực thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                MessageBox.Show("Lỗi: " + error, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (System.Exception ex)
        {
            MessageBox.Show("Lỗi kết nối: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [ObservableProperty]
    private UserDto? _editingUser;

    [ObservableProperty]
    private string? _editPassword;

    [ObservableProperty]
    private bool _isEditing;

    [RelayCommand]
    private void EditUser(UserDto user)
    {
        if (user == null) return;
        EditingUser = new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
        EditPassword = string.Empty;
        IsEditing = true;
    }

    [RelayCommand]
    private void CancelEdit()
    {
        IsEditing = false;
        EditingUser = null;
        EditPassword = string.Empty;
    }

    [RelayCommand]
    private async Task SaveEditAsync()
    {
        if (EditingUser == null) return;

        try
        {
            var dto = new { FullName = EditingUser.FullName, Role = EditingUser.Role, Password = EditPassword };
            var response = await _httpClient.PutAsJsonAsync($"/api/Admin/users/{EditingUser.Id}", dto);

            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Cập nhật thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                IsEditing = false;
                await LoadUsersAsync();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                MessageBox.Show("Lỗi: " + error, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (System.Exception ex)
        {
            MessageBox.Show("Lỗi kết nối: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task DeleteUserAsync(UserDto user)
    {
        if (user == null) return;

        var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa vĩnh viễn user {user.Email}?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result == MessageBoxResult.Yes)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"/api/Admin/users/{user.Id}");
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Đã xóa user thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadUsersAsync();
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    MessageBox.Show("Lỗi: " + error, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Lỗi kết nối: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    [RelayCommand]
    private void SelectUserTab()
    {
        IsUserTabSelected = true;
        IsReportTabSelected = false;
        _ = LoadUsersAsync();
    }

    [RelayCommand]
    private void SelectReportTab()
    {
        IsUserTabSelected = false;
        IsReportTabSelected = true;
        _ = LoadReportsAsync();
    }

    [RelayCommand]
    private async Task LoadReportsAsync()
    {
        IsLoading = true;
        try
        {
            var reports = await _httpClient.GetFromJsonAsync<List<ReportDto>>("/api/Admin/reports");
            if (reports != null)
            {
                Reports = new ObservableCollection<ReportDto>(reports);
            }

            // Sync stats
            try
            {
                var stats = await _httpClient.GetFromJsonAsync<System.Text.Json.JsonElement>("/api/Admin/stats");
                TotalUsers = stats.GetProperty("totalUsers").GetInt32();
                ActiveUsers = stats.GetProperty("activeUsers").GetInt32();
                TotalMatches = stats.GetProperty("totalMatches").GetInt32();
                TotalReports = stats.GetProperty("totalReports").GetInt32();
                UnresolvedReports = stats.GetProperty("unresolvedReports").GetInt32();
                VerifiedUsers = stats.GetProperty("verifiedUsers").GetInt32();
            }
            catch {}
        }
        catch (System.Exception ex)
        {
            MessageBox.Show("Lỗi lấy danh sách Báo cáo: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ResolveReportAsync(ReportDto report)
    {
        if (report == null) return;

        try
        {
            var response = await _httpClient.PostAsync($"/api/Admin/reports/{report.Id}/resolve", null);
            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Đã đánh dấu giải quyết báo cáo!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadReportsAsync();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                MessageBox.Show("Lỗi: " + error, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (System.Exception ex)
        {
            MessageBox.Show("Lỗi kết nối: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task ToggleReportedUserActiveAsync(ReportDto report)
    {
        if (report == null) return;

        try
        {
            var response = await _httpClient.PostAsync($"/api/Admin/users/{report.ReportedUser.Id}/toggle-active", null);
            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Thao tác tài khoản bị tố cáo thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadReportsAsync();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                MessageBox.Show("Lỗi: " + error, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (System.Exception ex)
        {
            MessageBox.Show("Lỗi kết nối: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void Logout()
    {
        var app = (App)System.Windows.Application.Current;
        var authService = app.Services.GetService(typeof(AuthService)) as AuthService;
        authService?.Logout();

        var loginVm = app.Services.GetService(typeof(LoginViewModel));
        CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(
            new DatingApp.Desktop.Messages.NavigationMessage(loginVm!)
        );
    }
}
