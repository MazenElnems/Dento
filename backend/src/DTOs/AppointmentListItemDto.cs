using Dento.Enums;

namespace Dento.DTOs;

public class AppointmentListItemDto
{
    public string Id { get; set; } = null!;
    public AppointmentStatus Status { get; set; }
    public AppointmentType AppointmentType { get; set; }
    
    public DateOnly SlotDate { get; set; }
    public TimeOnly SlotFrom { get; set; }
    public TimeOnly SlotTo { get; set; }
    
    public string DentistId { get; set; } = null!;
    public string DentistName { get; set; } = null!;
    
    public string? PaymentId { get; set; }
    public PaymentStatus? PaymentStatus { get; set; }
}
