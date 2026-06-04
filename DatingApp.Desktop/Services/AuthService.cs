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
        _httpClient = httpClientFactory.CreateClient("ApiClient");
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
        return false;
    }

    public void Logout()
    {
        CurrentToken = null;
        CurrentUser = null;
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }
}
