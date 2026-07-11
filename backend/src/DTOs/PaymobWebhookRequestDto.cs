using System.Text.Json.Serialization;

namespace Dento.DTOs;

public sealed class PaymobWebhookPayload
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("obj")]
    public PaymobWebhookRequest Obj { get; set; } = default!;
}

public sealed class PaymobWebhookRequest
{
    // Used for HMAC + Business Logic
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("amount_cents")]
    public int AmountCents { get; set; }

    [JsonPropertyName("created_at")]
    public string CreatedAt { get; set; } = string.Empty;

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonPropertyName("error_occured")]
    public bool ErrorOccured { get; set; }

    [JsonPropertyName("has_parent_transaction")]
    public bool HasParentTransaction { get; set; }

    [JsonPropertyName("integration_id")]
    public int IntegrationId { get; set; }

    [JsonPropertyName("is_3d_secure")]
    public bool Is3DSecure { get; set; }

    [JsonPropertyName("is_auth")]
    public bool IsAuth { get; set; }

    [JsonPropertyName("is_capture")]
    public bool IsCapture { get; set; }

    [JsonPropertyName("is_refunded")]
    public bool IsRefunded { get; set; }

    [JsonPropertyName("is_standalone_payment")]
    public bool IsStandalonePayment { get; set; }

    [JsonPropertyName("is_voided")]
    public bool IsVoided { get; set; }

    [JsonPropertyName("owner")]
    public int Owner { get; set; }

    [JsonPropertyName("pending")]
    public bool Pending { get; set; }

    [JsonPropertyName("order")]
    public PaymobOrderDto Order { get; set; } = default!;

    [JsonPropertyName("source_data")]
    public PaymobSourceDataDto SourceData { get; set; } = default!;

    [JsonPropertyName("signature")]
    public string Signature { get; set; } = string.Empty;

    [JsonPropertyName("refunded_amount_cents")]
    public int RefundedAmountCents { get; set; }

    [JsonPropertyName("captured_amount")]
    public int CapturedAmount { get; set; }
}