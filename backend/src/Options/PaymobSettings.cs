namespace Dento.Options;

public class PaymobSettings
{
    public required string BaseUrl { get; init; }
    public required string SecretKey { get; init; } 
    public required string PublicKey { get; init; }
    public required string ApiKey { get; init; }
    public required string HMAC { get; init; }
    public required string WebhookEndpointUrl { get; init; }
    public required string CreatePaymentIntentPath { get; init; }
    public required string CheckoutPageUrl { get; init; }    
}
