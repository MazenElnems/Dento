using Asp.Versioning;
using Dento.Constants;
using Dento.Controllers.Common;
using Dento.Data;
using Dento.DTOs;
using Dento.Enums;
using Dento.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dento.Controllers.v1;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion("1.0")]
public class SchedulesController : BaseApiController
{
    private readonly AppDbContext _context;

    public SchedulesController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/v1/schedules
    [HttpGet("{scheduleId}")]
    [Authorize(Roles = RoleNames.Patient)]
    public async Task<ActionResult<ApiResponse>> Get([FromRoute] string scheduleId)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        var availableSlots = await _context.Slots
            .Where(x => x.DentistAvailabilityId == scheduleId &&
                   x.Status == SlotStatus.Available           &&
                   x.Date >= today
            )
            .GroupBy(x => x.Date)
            .Select(x => new ScheduleDayDto
            {
                Date = x.Key,
                Slots = x.Select(y => new ScheduleSlotDto
                {
                    Id = y.Id,
                    From = y.From,
                    To = y.To
                }).OrderBy(z => z.From).ToList()
            })
            .OrderBy(x => x.Date).ToListAsync();

        return ApiResponse.SuccessResponse(availableSlots);
    }

    // PUT: api/v1/schedules
    [HttpPut("{scheduleId}")]
    [Authorize(Roles = RoleNames.Dentist)]
    public async Task<ActionResult<ApiResponse>> Update([FromRoute] string scheduleId,[FromBody] UpdateScheduleRequestDto request)
    {
        // Update schedule (detnist availability) without re-generate the slots
        var schedule = await _context.DentistAvailability.FirstOrDefaultAsync(x => x.Id == scheduleId);

        if (schedule == null)
            return NotFound(ApiResponse.ErrorResponse(ErrorCodes.ScheduleNotFound, StatusCodes.Status404NotFound));

        if (CurrentUser.Id != schedule.DentistId)
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse.ErrorResponse(ErrorCodes.NotOwned, StatusCodes.Status403Forbidden));

        schedule.SAT = request.SAT;
        schedule.SUN = request.SUN;
        schedule.MON = request.MON;
        schedule.TUE = request.TUE;
        schedule.WED = request.WED;
        schedule.THU = request.THU;
        schedule.FRI = request.FRI;

        schedule.FromHour = request.FromHour;
        schedule.ToHour = request.ToHour;
        schedule.SecondFromHour = request.SecondFromHour;
        schedule.SecondToHour = request.SecondToHour;
        schedule.SlotLengthInMinutes = schedule.SlotLengthInMinutes;

        schedule.IsActive = schedule.IsActive;

        await _context.SaveChangesAsync();

        return ApiResponse.SuccessResponse("schedule updated successfully");
    }

    [HttpPost("{scheduleId}/slots")]
    [Authorize(Roles = RoleNames.Dentist)]
    public async Task<ActionResult<ApiResponse>> GenerateSlots([FromRoute] string scheduleId, GenerateSlotsRequestDto request)
    {
        // generate the slots in the request response life time.
        var dentistAvailability = await _context.DentistAvailability.FirstOrDefaultAsync(x => x.Id == scheduleId);

        if (dentistAvailability == null)
            return NotFound(ApiResponse.ErrorResponse(ErrorCodes.ScheduleNotFound, StatusCodes.Status404NotFound));

        if(CurrentUser.Id != dentistAvailability.DentistId)
            return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse.ErrorResponse(ErrorCodes.NotOwned, StatusCodes.Status403Forbidden)
            );

        List<Slot> slots = new List<Slot>();

        var currentDate = request.StartDate;

        while (currentDate < request.EndDate)
        {
            if(currentDate.DayOfWeek == DayOfWeek.Saturday  && !dentistAvailability.SAT  ||
               currentDate.DayOfWeek == DayOfWeek.Sunday    && !dentistAvailability.SUN  ||
               currentDate.DayOfWeek == DayOfWeek.Monday    && !dentistAvailability.MON  ||
               currentDate.DayOfWeek == DayOfWeek.Tuesday   && !dentistAvailability.TUE  ||
               currentDate.DayOfWeek == DayOfWeek.Wednesday && !dentistAvailability.WED  ||
               currentDate.DayOfWeek == DayOfWeek.Thursday  && !dentistAvailability.THU  ||
               currentDate.DayOfWeek == DayOfWeek.Friday    && !dentistAvailability.FRI)
            {
                currentDate = currentDate.AddDays(1);
                continue;
            }

            var currentTime = dentistAvailability.FromHour; 
            while(currentTime < dentistAvailability.ToHour) 
            {
                slots.Add(new Slot
                {
                    Date = currentDate,
                    From = currentTime,
                    To = currentTime.AddMinutes(dentistAvailability.SlotLengthInMinutes),   
                    Status = SlotStatus.Available
                });

                currentTime = currentTime.AddMinutes(dentistAvailability.SlotLengthInMinutes);
            }

            var secondCurrentTime = dentistAvailability.SecondFromHour.HasValue ? dentistAvailability.SecondFromHour : null;
            while(dentistAvailability.HasTwoShifts && secondCurrentTime < dentistAvailability.SecondToHour)
            {
                slots.Add(new Slot
                {
                    Date = currentDate,
                    From = secondCurrentTime.Value,
                    To = secondCurrentTime.Value.AddMinutes(dentistAvailability.SlotLengthInMinutes),
                    Status = SlotStatus.Available
                });

                secondCurrentTime = secondCurrentTime.Value.AddMinutes(dentistAvailability.SlotLengthInMinutes);
            }

            currentDate = currentDate.AddDays(1);
        }

        dentistAvailability.Slots.AddRange(slots);
        await _context.SaveChangesAsync();

        return ApiResponse.SuccessResponse("Slots generated successfully");
    }
}
