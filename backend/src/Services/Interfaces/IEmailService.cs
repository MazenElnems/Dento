namespace Dento.Services.Interfaces;

public interface IEmailService
{
    Task SendVerificationEmailAsync(string userName, string email, string verificationCode, int expirationMinutes);
}
