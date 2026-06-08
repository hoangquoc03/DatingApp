using System.Net;
using System.Net.Mail;

namespace DatingApp.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendPasswordResetAsync(string toEmail, string resetLink)
        {
            var smtpHost = _config["Smtp:Host"];
            var smtpPort = int.TryParse(_config["Smtp:Port"], out var parsedPort) ? parsedPort : 587;
            var smtpUser = _config["Smtp:Username"];
            var smtpPass = _config["Smtp:Password"];
            var from = _config["Smtp:FromEmail"] ?? "no-reply@datingapp.local";
            var appName = _config["Smtp:FromName"] ?? "DatingApp";

            if (string.IsNullOrWhiteSpace(smtpHost))
            {
                _logger.LogWarning("SMTP not configured. Password reset link for {Email}: {Link}", toEmail, resetLink);
                return;
            }

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = true,
                Credentials = string.IsNullOrWhiteSpace(smtpUser)
                    ? CredentialCache.DefaultNetworkCredentials
                    : new NetworkCredential(smtpUser, smtpPass)
            };

            using var message = new MailMessage
            {
                From = new MailAddress(from, appName),
                Subject = "Reset your DatingApp password",
                Body = $"Click this link to reset your password: {resetLink}\n\nThis link will expire in 15 minutes.",
                IsBodyHtml = false
            };

            message.To.Add(toEmail);
            await client.SendMailAsync(message);
        }

        public async Task SendEmailVerificationOtpAsync(string toEmail, string otp)
        {
            var smtpHost = _config["Smtp:Host"];
            var smtpPort = int.TryParse(_config["Smtp:Port"], out var parsedPort) ? parsedPort : 587;
            var smtpUser = _config["Smtp:Username"];
            var smtpPass = _config["Smtp:Password"];
            var from = _config["Smtp:FromEmail"] ?? "no-reply@datingapp.local";
            var appName = _config["Smtp:FromName"] ?? "DatingApp";

            if (string.IsNullOrWhiteSpace(smtpHost))
            {
                _logger.LogWarning("SMTP not configured. Email verification OTP for {Email}: {Otp}", toEmail, otp);
                return;
            }

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = true,
                Credentials = string.IsNullOrWhiteSpace(smtpUser)
                    ? CredentialCache.DefaultNetworkCredentials
                    : new NetworkCredential(smtpUser, smtpPass)
            };

            using var message = new MailMessage
            {
                From = new MailAddress(from, appName),
                Subject = "Verify your DatingApp email",
                Body = $"Your verification code is: {otp}\n\nThis code will expire in 10 minutes.",
                IsBodyHtml = false
            };

            message.To.Add(toEmail);
            await client.SendMailAsync(message);
        }
    }
}
