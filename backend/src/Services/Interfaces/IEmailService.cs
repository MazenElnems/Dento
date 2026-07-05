namespace Dento.Services.Interfaces;

public interface IEmailService
{
    Task SendVerificationEmailAsync(string userName, string email, string verificationCode, int expirationMinutes = 30);
    Task SendResetPasswordEmailAsync(string userName, string email, string resetPasswordUrl, int expirationMinutes = 30);
}
