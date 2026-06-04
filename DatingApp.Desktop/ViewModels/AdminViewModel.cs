using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
    private int _totalUsers;

    [ObservableProperty]
    private int _activeUsers;

    [ObservableProperty]
    private int _totalMatches;

    [ObservableProperty]
    private int _totalReports;

    [ObservableProperty]
    private int _unresolvedReports;

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
}
