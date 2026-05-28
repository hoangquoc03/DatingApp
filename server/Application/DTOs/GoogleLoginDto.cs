namespace DatingApp.DTOs
{
    // Frontend useGoogleLogin() trả về access_token (OAuth 2.0 flow)
    // KHÔNG phải credential (ID token flow của @react-oauth/google)
    public class GoogleLoginDto
    {
        public string AccessToken { get; set; } = "";
    }
}