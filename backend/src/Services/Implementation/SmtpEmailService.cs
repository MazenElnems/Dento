using Dento.Options;
using Dento.Services.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Dento.Services.Implementation;

public class SmtpEmailService : IEmailService
{
    private readonly IWebHostEnvironment _env;
    private readonly MailSettings _emailSettings;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IWebHostEnvironment webHostEnvironment, IOptions<MailSettings> emailSettings, ILogger<SmtpEmailService> logger)
    {
        _env = webHostEnvironment;
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task SendVerificationEmailAsync(string userName, string email, string verificationCode, int expirationMinutes)
    {
        var templatePath = Path.Combine(_env.WebRootPath, "EmailTemplates", "VerificationEmail.html");

        var template = await File.ReadAllTextAsync(templatePath);

        var body = template
            .Replace("{{UserName}}", userName)
            .Replace("{{Email}}", email)
            .Replace("{{VerificationCode}}", verificationCode)
            .Replace("{{ExpirationMinutes}}", expirationMinutes.ToString());

        await SendAsync(email, userName, "Email Verification", body);
    }

    private async Task SendAsync(string to, string toName, string subject, string body)
    {
        var message = new MimeMessage();

        message.From.Add(new MailboxAddress(_emailSettings.DisplayName, _emailSettings.Email));
        message.To.Add(new MailboxAddress(toName, to));
        message.Subject = subject;

        var builder = new BodyBuilder();

        var logo = await builder.LinkedResources.AddAsync(Path.Combine(_env.WebRootPath, "logo-removebg.png"));
        logo.IsAttachment = false;

        logo.ContentId = "dento-logo";
        builder.HtmlBody = body;

        message.Body = builder.ToMessageBody();

        using var smtpClient = new SmtpClient();

        try
        {
            // Connect to SMTP Server
            await smtpClient.ConnectAsync(_emailSettings.Host, _emailSettings.Port, SecureSocketOptions.StartTls);

            // Authentication
            await smtpClient.AuthenticateAsync(_emailSettings.Email, _emailSettings.Password);

            // Send Message
            await smtpClient.SendAsync(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "an error occurs while sending email to {Email}", to);
        }
        finally
        {
            // Disconnect
            await smtpClient.DisconnectAsync(true);
        }
    }
}
