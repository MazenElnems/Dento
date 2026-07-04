namespace Dento.Options;

public class JwtTokenSettings
{
    public string SecretKey { get; set; } = null!; 
    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public int ExpiresInMinutes { get; set; }
}
