using Dento.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dento.Models;

[PrimaryKey(nameof(EventId))]
[Table("PaymentEventLogs")]
public class PaymentEvent
{
    public string EventId { get; set; } = Guid.NewGuid().ToString();

    [ForeignKey(nameof(Payment))]
    public string PaymentId { get; set; } = default!;
    public Payment Payment { get; set; } = default!;

    public PaymentEventType Type { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string? RawPayload { get; set; }
}
