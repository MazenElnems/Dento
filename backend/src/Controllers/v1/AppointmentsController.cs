using Asp.Versioning;
using Dento.Constants;
using Dento.Controllers.Common;
using Dento.Data;
using Dento.DTOs;
using Dento.Enums;
using Dento.Jobs;
using Dento.Models;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dento.Controllers.v1;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion("1.0")]
public class AppointmentsController : BaseApiController
{
    private readonly AppDbContext _context;
    private readonly IReleaseLockedSlotJob _releaseLockedSlotJob;
    private readonly ILogger<AppointmentsController> _logger;

    public AppointmentsController(AppDbContext context, IReleaseLockedSlotJob releaseLockedSlotJob, ILogger<AppointmentsController> logger)
    {
        _context = context;
        _releaseLockedSlotJob = releaseLockedSlotJob;
        _logger = logger;
    }

    /// <summary>
    /// Books a time slot for the authenticated patient.
    /// </summary>
    /// <remarks>
    /// The booking behavior depends on the chosen payment type:
    /// - **Online**: Locks the slot for 10 minutes. The patient must complete payment
    ///   within that window by calling <c>POST /api/v1/Payments/create-payment-intent</c>.
    /// - **Cash**: Immediately confirms the appointment and books the slot.
    ///   Payment remains pending until a Receptionist confirms it at the clinic.
    /// </remarks>
    [HttpPost]
    [Authorize(Roles = RoleNames.Patient)]
    public async Task<ActionResult<ApiResponse>> Book(BookAppointmentRequestDto request)
    {
        _logger.LogInformation("Booking attempt | SlotId: {SlotId} | PaymentType: {PaymentType} | PatientId: {PatientId}",
            request.SlotId, request.PaymentType, CurrentUser.Id);

        var slot = await _context.Slots
            .Include(s => s.DentistAvailability)
            .FirstOrDefaultAsync(s => s.Id == request.SlotId);

        if (slot == null)
            return NotFound(ApiResponse.ErrorResponse(ErrorCodes.SlotIsNotFound, StatusCodes.Status404NotFound));

        if (slot.Status != SlotStatus.Available)
            return Conflict(ApiResponse.ErrorResponse(ErrorCodes.SlotIsNotAvailable, StatusCodes.Status409Conflict));

        var appointment = new Appointment
        {
            PatientId  = CurrentUser.Id,
            SlotId     = slot.Id,
            CreatedAt  = DateTime.UtcNow,
            DentistId  = slot.DentistAvailability!.DentistId
        };

        if (request.PaymentType == PaymentType.Cash)
        {
            // Cash: immediately book the slot, appointment stays Pending until payment is processed
            slot.Status = SlotStatus.Booked;
            slot.LockedUntil = null;
            appointment.Status = AppointmentStatus.Pending;

            _context.Appointments.Add(appointment);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict(ApiResponse.ErrorResponse(ErrorCodes.SlotAppointmentConflict, StatusCodes.Status409Conflict));
            }

            _logger.LogInformation("Cash booking completed | AppointmentId: {AppointmentId} | SlotId: {SlotId}",
                appointment.Id, slot.Id);

            return ApiResponse.SuccessResponse(new
            {
                appointment.Id,
                appointment.Status,
                SlotId     = slot.Id,
                SlotStatus = slot.Status,
                PaymentType = request.PaymentType,
                Message = "Appointment booked. Please proceed to create a cash payment."
            }, "Appointment booked with cash payment. Please create payment to confirm.");
        }
        else
        {
            // Online: lock the slot for 10 minutes
            slot.Status = SlotStatus.Locked;
            slot.LockedUntil = DateTime.UtcNow.AddMinutes(10);
            appointment.Status = AppointmentStatus.Pending;

            // Release the slot if payment is not completed within 10 minutes
            BackgroundJob.Schedule(
                () => _releaseLockedSlotJob.ExecuteAsync(appointment.Id),
                DateTime.UtcNow.AddMinutes(10)
            );

            _context.Appointments.Add(appointment);

            try
            {
                await _context.SaveChangesAsync();
                return ApiResponse.SuccessResponse(new
                {
                    appointment.Id,
                    appointment.Status,
                    SlotId     = slot.Id,
                    SlotStatus = slot.Status,
                    slot.LockedUntil,
                    PaymentDeadline = slot.LockedUntil,
                    PaymentType = request.PaymentType
                }, "Appointment booked successfully. Please complete payment within 10 minutes.");
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict(ApiResponse.ErrorResponse(ErrorCodes.SlotAppointmentConflict, StatusCodes.Status409Conflict));
            }
        }
    }

    /// <summary>
    /// Gets the details and current status of an appointment.
    /// </summary>
    /// <remarks>
    /// Returns full appointment details including slot info, dentist info, and payment status.
    /// Only the patient who booked the appointment can view it.
    /// Use this endpoint to poll the status after booking and after initiating payment.
    /// </remarks>
    /// <param name="id">The appointment ID.</param>
    [HttpGet("{id}")]
    [Authorize(Roles = RoleNames.Patient)]
    public async Task<ActionResult<ApiResponse>> GetById([FromRoute] string id)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Slot)
            .Include(a => a.Dentist)
            .Include(a => a.Payment)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appointment == null)
            return NotFound(ApiResponse.ErrorResponse(ErrorCodes.AppointmentNotFound, StatusCodes.Status404NotFound));

        // Ensure the appointment belongs to the requesting patient
        if (appointment.PatientId != CurrentUser.Id)
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse.ErrorResponse(ErrorCodes.AppointmentNotOwned, StatusCodes.Status403Forbidden));

        var response = new AppointmentDetailsResponseDto
        {
            Id              = appointment.Id,
            Status          = appointment.Status,
            AppointmentType = appointment.AppointmentType,
            CreatedAt       = appointment.CreatedAt,
            ConfirmedAt     = appointment.ConfirmedAt,
            CanceledAt      = appointment.CanceledAt,

            SlotId          = appointment.Slot.Id,
            SlotDate        = appointment.Slot.Date,
            SlotFrom        = appointment.Slot.From,
            SlotTo          = appointment.Slot.To,
            SlotStatus      = appointment.Slot.Status,
            SlotLockedUntil = appointment.Slot.LockedUntil,

            DentistId        = appointment.Dentist.Id,
            DentistName      = appointment.Dentist.FullName,
            DentistSpecialty = appointment.Dentist.Specialty,
            ConsultationFee  = appointment.Dentist.ConsultationFee,

            PaymentId       = appointment.Payment?.Id,
            PaymentStatus   = appointment.Payment?.Status,
            PaymentAmount   = appointment.Payment?.Amount,
            PaymentCurrency = appointment.Payment?.Currency,
            PaymentMethod   = appointment.Payment?.PaymentMethod
        };

        return ApiResponse.SuccessResponse(response);
    }

    /// <summary>
    /// Gets a list of appointments for the currently authenticated patient.
    /// </summary>
    /// <param name="status">Optional filter for status (e.g. "upcoming" or "completed").</param>
    [HttpGet("my-appointments")]
    [Authorize(Roles = RoleNames.Patient)]
    public async Task<ActionResult<ApiResponse>> GetMyAppointments([FromQuery] string? status)
    {
        var query = _context.Appointments
            .Include(a => a.Slot)
            .Include(a => a.Dentist)
            .Include(a => a.Payment)
            .Where(a => a.PatientId == CurrentUser.Id);

        if (!string.IsNullOrEmpty(status))
        {
            if (status.Equals("upcoming", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(a => a.Status == AppointmentStatus.Pending || a.Status == AppointmentStatus.Confirmed);
            }
            else if (status.Equals("completed", StringComparison.OrdinalIgnoreCase))
            {
                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                query = query.Where(a => a.Status == AppointmentStatus.Canceled || a.Status == AppointmentStatus.Failed || (a.Status == AppointmentStatus.Confirmed && a.Slot.Date < today));
            }
        }

        var appointments = await query
            .OrderByDescending(a => a.Slot.Date)
            .ThenByDescending(a => a.Slot.From)
            .Select(a => new AppointmentListItemDto
            {
                Id = a.Id,
                Status = a.Status,
                AppointmentType = a.AppointmentType,
                SlotDate = a.Slot.Date,
                SlotFrom = a.Slot.From,
                SlotTo = a.Slot.To,
                DentistId = a.Dentist.Id,
                DentistName = a.Dentist.FullName,
                PaymentId = a.Payment != null ? a.Payment.Id : null,
                PaymentStatus = a.Payment != null ? a.Payment.Status : null
            })
            .ToListAsync();

        return ApiResponse.SuccessResponse(appointments);
    }
}
