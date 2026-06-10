using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using DatingApp.Desktop.Models;

namespace DatingApp.Desktop.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;
    public string? CurrentToken { get; private set; }
    public UserProfile? CurrentUser { get; private set; }

    public AuthService(IHttpClientFactory httpClientFactory)
    {
        // Dùng "PublicClient" — KHÔNG có AuthTokenHandler để tránh circular dependency.
        // Login và Register không cần Bearer token.
        _httpClient = httpClientFactory.CreateClient("PublicClient");
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        var request = new LoginRequest { Email = email, Password = password };
        var response = await _httpClient.PostAsJsonAsync("/api/Auth/login", request);

        if (response.IsSuccessStatusCode)
        {
            var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
            if (authResponse != null && !string.IsNullOrEmpty(authResponse.Token))
            {
                CurrentToken = authResponse.Token;
                CurrentUser = authResponse.User;
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CurrentToken);
                return true;
            }
        }
        else
        {
            var content = await response.Content.ReadAsStringAsync();
            if (content.Contains("EMAIL_NOT_VERIFIED"))
            {
                throw new Exception("EMAIL_NOT_VERIFIED");
            }
        }
        return false;
    }

    public async Task<bool> RegisterAsync(RegisterDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/Auth/register", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> VerifyEmailAsync(string email, string otp)
    {
        var request = new { Email = email, Otp = otp };
        var response = await _httpClient.PostAsJsonAsync("/api/Auth/verify-email", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ForgotPasswordAsync(string email)
    {
        var request = new { Email = email };
        var response = await _httpClient.PostAsJsonAsync("/api/Auth/forgot-password", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ResetPasswordAsync(string token, string newPassword)
    {
        var request = new { Token = token, NewPassword = newPassword };
        var response = await _httpClient.PostAsJsonAsync("/api/Auth/reset-password", request);
        return response.IsSuccessStatusCode;
    }

    public void Logout()
    {
        CurrentToken = null;
        CurrentUser = null;
        _httpClient.DefaultRequestHeaders.Authorization = null;
        ClearSession();
    }

    private static string GetSessionFilePath()
    {
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AuraDatingApp");
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }
        return Path.Combine(folder, "session.json");
    }

    public async Task SaveSessionAsync(string token, UserProfile user)
    {
        try
        {
            var filePath = GetSessionFilePath();
            var sessionData = new AuthResponse { Token = token, User = user };
            var json = JsonSerializer.Serialize(sessionData);
            await File.WriteAllTextAsync(filePath, json);
        }
        catch {}
    }

    public void ClearSession()
    {
        try
        {
            var filePath = GetSessionFilePath();
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch {}
    }

    public async Task<bool> TryAutoLoginAsync()
    {
        try
        {
            var filePath = GetSessionFilePath();
            if (!File.Exists(filePath)) return false;

            var json = await File.ReadAllTextAsync(filePath);
            var sessionData = JsonSerializer.Deserialize<AuthResponse>(json);
            if (sessionData == null || string.IsNullOrEmpty(sessionData.Token)) return false;

            // Set token tạm thời để kiểm tra
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessionData.Token);
            
            // Gọi API profile để xác minh token
            var response = await _httpClient.GetAsync("/api/User/profile");
            if (response.IsSuccessStatusCode)
            {
                var userDto = await response.Content.ReadFromJsonAsync<UserDto>();
                if (userDto != null)
                {
                    // Cập nhật thông tin profile mới nhất nếu có thay đổi
                    sessionData.User.FullName = userDto.FullName ?? sessionData.User.FullName;
                    sessionData.User.AvatarUrl = userDto.AvatarUrl ?? sessionData.User.AvatarUrl;
                    sessionData.User.IsOnboarded = userDto.IsOnboarded;
                    sessionData.User.ProfileCompletionScore = userDto.ProfileCompletionScore;

                    // Lưu lại session đã cập nhật
                    await SaveSessionAsync(sessionData.Token, sessionData.User);
                }

                CurrentToken = sessionData.Token;
                CurrentUser = sessionData.User;
                return true;
            }
        }
        catch {}

        // Reset if failed
        _httpClient.DefaultRequestHeaders.Authorization = null;
        ClearSession(); // Xoá session hỏng hoặc hết hạn
        return false;
    }
}
