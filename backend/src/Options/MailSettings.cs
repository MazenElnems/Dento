namespace Dento.Options;

public class MailSettings
{
    public string Email { get; init; } = default!;
    public string DisplayName { get; init; } = default!;
    public string Host { get; init; } = default!;
    public int Port { get; init; }
    public string Password { get; init; } = default!;
}
