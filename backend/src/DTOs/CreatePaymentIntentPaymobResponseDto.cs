namespace Dento.DTOs;

public class CreatePaymentIntentPaymobResponseDto
{
    public string ClientSecret { get; init; } = default!;
    public string SpecialReference { get; init; } = default!;
    public long IntentionOrderId { get; init; } 
}
