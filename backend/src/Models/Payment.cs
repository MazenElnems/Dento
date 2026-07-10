using Dento.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dento.Models;

public class Payment
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    public string IdempotencyKey { get; set; } = default!;

    public string? ClientSecret { get; set; } = default!;
    public long? IntentionId{ get; set; }
    public long? TransactionId { get; set; } // // Transaction after payment

    public decimal Amount { get; set; }
    public string Currency { get; set; } = "EGP";

    public string? FauilerReason { get; set; }


    public PaymentStatus Status { get; set; }
    public PaymentMethod PaymentMethod { get; set; }

    public string? PayerEmail { get; set; }
    public string? PayerName { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public List<PaymentEvent> Events { get; set; } = [];
}
