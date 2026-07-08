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

    public AppointmentsController(AppDbContext context, IReleaseLockedSlotJob releaseLockedSlotJob)
    {
        _context = context;
        _releaseLockedSlotJob = releaseLockedSlotJob;
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.Patient)]
    public async Task<ActionResult<ApiResponse>> Book(BookAppointmentRequestDto request)
    {
        var slot = await _context.Slots.FirstOrDefaultAsync(s => s.Id == request.SlotId);

        if (slot == null)
            return NotFound(ApiResponse.ErrorResponse(ErrorCodes.SlotIsNotFound, StatusCodes.Status404NotFound));

        if (slot.Status != SlotStatus.Available)
            return Conflict(ApiResponse.ErrorResponse(ErrorCodes.SlotIsNotAvailable, StatusCodes.Status409Conflict));

        slot.Status = SlotStatus.Locked;
        slot.LockedUntil = DateTime.UtcNow.AddMinutes(3);

        BackgroundJob.Schedule(() => _releaseLockedSlotJob.ExecuteAsync(slot.Id), DateTime.UtcNow.AddMinutes(3));

        var appointment = new Appointment
        {
            PatientId = CurrentUser.Id,
            SlotId = slot.Id,
            Status = AppointmentStatus.Pending,
            CreatedAt = DateTime.UtcNow,
        };

        _context.Appointments.Add(appointment);

        try
        {
            await _context.SaveChangesAsync();
            return ApiResponse.SuccessResponse("Appointment Booked Successfully");
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(ApiResponse.ErrorResponse(ErrorCodes.SlotAppointmentConflict, StatusCodes.Status409Conflict));
        }
    }
}
