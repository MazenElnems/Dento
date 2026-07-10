using Dento.Options;
using Dento.Services.Interfaces;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace Dento.Services.Implementation;

public class PaymobHmacVarifier : IPaymobHmacVarifier
{
    private readonly PaymobSettings _paymob;
    private readonly ILogger<PaymobHmacVarifier> _logger;

    public PaymobHmacVarifier(IOptions<PaymobSettings> paymob, ILogger<PaymobHmacVarifier> logger)
    {
        _paymob = paymob.Value;
        _logger = logger;
    }

    public bool Verify(string combinedQueryParameters, string receivedHmac)
    {
        _logger.LogInformation("HMAC verification started for webhook payload");

        // Calculate the HMAC using your secret
        using var hmacSha512 = new HMACSHA512(Encoding.UTF8.GetBytes(_paymob.HMAC));

        var hash = hmacSha512.ComputeHash(Encoding.UTF8.GetBytes(combinedQueryParameters));
        var calculatedHmac = Convert.ToHexString(hash).ToLowerInvariant();

        // Compare the calculated HMAC with the received one
        var isValid = CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(calculatedHmac),
            Encoding.UTF8.GetBytes(receivedHmac)
        );

        _logger.LogInformation("HMAC verification completed | IsValid: {IsValid}", isValid);

        return isValid;
    }
}
