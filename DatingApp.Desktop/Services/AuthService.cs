using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
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
    }
}
