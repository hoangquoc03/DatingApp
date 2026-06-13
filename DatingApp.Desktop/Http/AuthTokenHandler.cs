using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using DatingApp.Desktop.Services;
using DatingApp.Desktop.ViewModels;
using CommunityToolkit.Mvvm.Messaging;

namespace DatingApp.Desktop.Http;

/// <summary>
/// Automatically attaches the Bearer JWT token from AuthService to every outgoing HTTP request.
/// Intercepts 401 Unauthorized responses to perform silent token refreshing.
/// </summary>
public class AuthTokenHandler : DelegatingHandler
{
    private readonly AuthService _authService;

    public AuthTokenHandler(AuthService authService)
    {
        _authService = authService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = _authService.CurrentToken;
        System.Diagnostics.Debug.WriteLine(
            $"[AuthTokenHandler] URL={request.RequestUri} | Token={(string.IsNullOrEmpty(token) ? "NULL/EMPTY" : token[..20] + "...")}");

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            System.Diagnostics.Debug.WriteLine("[AuthTokenHandler] 401 detected, attempting silent refresh...");
            
            // Try to refresh token
            var refreshed = await _authService.RefreshAccessTokenAsync();
            if (refreshed)
            {
                System.Diagnostics.Debug.WriteLine("[AuthTokenHandler] Token refreshed successfully. Retrying request.");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authService.CurrentToken);
                return await base.SendAsync(request, cancellationToken);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[AuthTokenHandler] Token refresh failed. Logging out.");
                
                // Force logout on dispatcher thread
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    _authService.Logout();
                    
                    var app = (App)System.Windows.Application.Current;
                    if (app.Services.GetService(typeof(LoginViewModel)) is LoginViewModel loginVm)
                    {
                        loginVm.ErrorMessage = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.";
                        CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(
                            new DatingApp.Desktop.Messages.NavigationMessage(loginVm)
                        );
                    }
                });
            }
        }

        return response;
    }
}
