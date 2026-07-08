namespace Dento.Models;

public class AccessToken
{
    public string Token { get; set; } = null!;
    public DateTime ExpirationDate { get; set; }
}
