using Asp.Versioning;
using Dento.Controllers.Common;
using Dento.Data;
using Dento.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dento.Controllers.v1;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion("1.0")]
public class DentistsController : BaseApiController
{
    private readonly AppDbContext _context;

    public DentistsController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Lists all available dentists.
    /// </summary>
    /// <remarks>
    /// Returns dentists whose schedules are active. Use the returned <c>ScheduleId</c>
    /// to call <c>GET /api/v1/Schedules/{scheduleId}</c> to view that dentist's available time slots.
    /// </remarks>
    /// <returns>A list of dentists with their name, specialty, consultation fee, and schedule ID.</returns>
    /// <response code="200">Dentists retrieved successfully.</response>
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<ApiResponse>> GetAll()
    {
        var dentists = await _context.Dentists
            .Include(d => d.DentistAvailability)
            .Where(d => d.DentistAvailability != null && d.DentistAvailability.IsActive)
            .Select(d => new DentistListItemDto
            {
                Id               = d.Id,
                FullName         = d.FirstName + " " + d.LastName,
                Specialty        = d.Specialty,
                ConsultationFee  = d.ConsultationFee,
                ScheduleId       = d.DentistAvailability.Id,
                ImageUrl         = d.ImageUrl,
                YearsOfExperience = d.YearsOfExperience
            })
            .OrderBy(d => d.FullName)
            .ToListAsync();

        return ApiResponse.SuccessResponse(dentists);
    }
}
