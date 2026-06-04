using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using DatingApp.Desktop.Services;

namespace DatingApp.Desktop.Http;

/// <summary>
/// Automatically attaches the Bearer JWT token from AuthService to every outgoing HTTP request.
/// This solves the 401 problem where each ViewModel creates its own HttpClient
/// from the factory and does not have the token set.
/// </summary>
public class AuthTokenHandler : DelegatingHandler
{
    private readonly AuthService _authService;

    public AuthTokenHandler(AuthService authService)
    {
        _authService = authService;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = _authService.CurrentToken;
        System.Diagnostics.Debug.WriteLine(
            $"[AuthTokenHandler] URL={request.RequestUri} | Token={(string.IsNullOrEmpty(token) ? "NULL/EMPTY" : token[..20] + "...")}");

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        return base.SendAsync(request, cancellationToken);
    }
}
