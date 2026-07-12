using Asp.Versioning;
using Dento.Constants;
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
public class PatientsController : BaseApiController
{
    private readonly AppDbContext _context;

    public PatientsController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets a list of all patients in the system.
    /// </summary>
    /// <returns>A list of patients with their basic details and medical record ID.</returns>
    [HttpGet]
    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Dentist + "," + RoleNames.Receptionist)]
    public async Task<ActionResult<ApiResponse>> GetAllPatients()
    {
        var patients = await _context.Patients
            .Include(p => p.MedicalRecord)
            .Select(p => new PatientListItemDto
            {
                Id = p.Id,
                FullName = p.FullName,
                Email = p.Email!,
                PhoneNumber = p.PhoneNumber,
                Gender = p.Gender,
                DateOfBirth = p.DateOfBirth,
                Age = p.Age,
                MedicalRecordId = p.MedicalRecord != null ? p.MedicalRecord.Id : null
            })
            .ToListAsync();

        return ApiResponse.SuccessResponse(patients);
    }
}
