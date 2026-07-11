using Dento.Enums;

namespace Dento.DTOs;

public class AppointmentDetailsResponseDto
{
    public string Id { get; set; } = default!;
    public AppointmentStatus Status { get; set; }
    public AppointmentType AppointmentType { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? CanceledAt { get; set; }

    // Slot info
    public string SlotId { get; set; } = default!;
    public DateOnly SlotDate { get; set; }
    public TimeOnly SlotFrom { get; set; }
    public TimeOnly SlotTo { get; set; }
    public SlotStatus SlotStatus { get; set; }
    public DateTime? SlotLockedUntil { get; set; }

    // Dentist info
    public string DentistId { get; set; } = default!;
    public string DentistName { get; set; } = default!;
    public string DentistSpecialty { get; set; } = default!;
    public decimal? ConsultationFee { get; set; }

    // Payment info (nullable — payment may not exist yet)
    public string? PaymentId { get; set; }
    public PaymentStatus? PaymentStatus { get; set; }
    public decimal? PaymentAmount { get; set; }
    public string? PaymentCurrency { get; set; }
}
