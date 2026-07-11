using Asp.Versioning;
using Dento.Constants;
using Dento.Controllers.Common;
using Dento.DTOs;
using Dento.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dento.Controllers.V1;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion("1.0")]
public class MedicalRecordsController : BaseApiController
{
    private readonly IMedicalRecordService _medicalRecordService;

    public MedicalRecordsController(IMedicalRecordService medicalRecordService)
    {
        _medicalRecordService = medicalRecordService;
    }

    /// <summary>
    /// Gets the medical record of a patient.
    /// </summary>
    /// <param name="patientId">The ID of the patient.</param>
    /// <returns>The patient's medical record with categorized visits.</returns>
    [HttpGet("{patientId}")]
    [Authorize(Roles = RoleNames.Dentist + "," + RoleNames.Patient + "," + RoleNames.Admin)]
    public async Task<ActionResult<ApiResponse>> GetMedicalRecord(string patientId)
    {
        // For patients, ensure they can only fetch their own medical record
        if (User.IsInRole(RoleNames.Patient) && CurrentUser.Id != patientId)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse.ErrorResponse(ErrorCodes.NotOwned, StatusCodes.Status403Forbidden));
        }

        var record = await _medicalRecordService.GetMedicalRecordByPatientIdAsync(patientId);
        if (record == null)
            return NotFound(ApiResponse.ErrorResponse("Medical record not found", StatusCodes.Status404NotFound));

        return ApiResponse.SuccessResponse(record);
    }

    /// <summary>
    /// Adds a visit record (diagnosis, prescriptions, procedures) to a patient's medical record.
    /// </summary>
    /// <param name="patientId">The ID of the patient.</param>
    /// <param name="dto">The visit record details including appointment ID.</param>
    /// <returns>The created visit record.</returns>
    [HttpPost("{patientId}/visits")]
    [Authorize(Roles = RoleNames.Dentist)]
    public async Task<ActionResult<ApiResponse>> AddVisitRecord(string patientId, CreateVisitRecordDto dto)
    {
        var visitRecord = await _medicalRecordService.AddVisitRecordAsync(patientId, dto);
        return ApiResponse.SuccessResponse(visitRecord, "Visit record added successfully.");
    }
}
