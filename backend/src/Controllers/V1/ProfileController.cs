using Asp.Versioning;
using Dento.Constants;
using Dento.Controllers.Common;
using Dento.Data;
using Dento.DTOs;
using Dento.Enums;
using Dento.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dento.Controllers.V1;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion("1.0")]
[Authorize]
public class ProfileController : BaseApiController
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _context;
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(UserManager<ApplicationUser> userManager, AppDbContext context, ILogger<ProfileController> logger)
    {
        _userManager = userManager;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Gets the profile data of the currently authenticated user.
    /// Returns additional fields (Specialty, ConsultationFee) when the user is a Dentist.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse>> GetProfile()
    {
        var user = await _userManager.FindByIdAsync(CurrentUser.Id);
        if (user == null)
            return NotFound(ApiResponse.ErrorResponse("User not found", StatusCodes.Status404NotFound));

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Unknown";

        var profile = new UserProfileDto
        {
            Id = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            MiddleName = user.MiddleName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            Role = role
        };

        // If the user is a dentist, populate the dentist-specific fields
        if (role == RoleNames.Dentist && user is Dentist dentist)
        {
            profile.Specialty = dentist.Specialty;
            profile.ConsultationFee = dentist.ConsultationFee;
        }

        return ApiResponse.SuccessResponse(profile);
    }

    /// <summary>
    /// Changes the user's password.
    /// </summary>
    [HttpPut("password")]
    public async Task<ActionResult<ApiResponse>> ChangePassword(ChangePasswordDto request)
    {
        var user = await _userManager.FindByIdAsync(CurrentUser.Id);
        if (user == null)
            return NotFound(ApiResponse.ErrorResponse("User not found", StatusCodes.Status404NotFound));

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

        if (!result.Succeeded)
        {
            _logger.LogWarning("Change password failed for UserId: {UserId} | Errors: {Errors}", CurrentUser.Id, string.Join(", ", result.Errors.Select(e => e.Code)));
            return BadRequest(ApiResponse.ErrorResponse(result.Errors.First().Code, StatusCodes.Status400BadRequest));
        }

        return ApiResponse.SuccessResponse("Password changed successfully.");
    }

    /// <summary>
    /// Changes the user's email address.
    /// </summary>
    [HttpPut("email")]
    public async Task<ActionResult<ApiResponse>> ChangeEmail(ChangeEmailDto request)
    {
        var user = await _userManager.FindByIdAsync(CurrentUser.Id);
        if (user == null)
            return NotFound(ApiResponse.ErrorResponse("User not found", StatusCodes.Status404NotFound));

        var existingUser = await _userManager.FindByEmailAsync(request.NewEmail);
        if (existingUser != null && existingUser.Id != user.Id)
        {
            return BadRequest(ApiResponse.ErrorResponse("Email is already in use.", StatusCodes.Status400BadRequest));
        }

        var token = await _userManager.GenerateChangeEmailTokenAsync(user, request.NewEmail);
        var result = await _userManager.ChangeEmailAsync(user, request.NewEmail, token);

        if (!result.Succeeded)
        {
            return BadRequest(ApiResponse.ErrorResponse(result.Errors.First().Code, StatusCodes.Status400BadRequest));
        }

        // Must update username to match the email since username is email
        await _userManager.SetUserNameAsync(user, request.NewEmail);

        return ApiResponse.SuccessResponse("Email changed successfully.");
    }

    /// <summary>
    /// Changes the user's name (First, Middle, Last).
    /// </summary>
    [HttpPut("name")]
    public async Task<ActionResult<ApiResponse>> ChangeName(ChangeNameDto request)
    {
        var user = await _userManager.FindByIdAsync(CurrentUser.Id);
        if (user == null)
            return NotFound(ApiResponse.ErrorResponse("User not found", StatusCodes.Status404NotFound));

        user.FirstName = request.FirstName;
        user.MiddleName = request.MiddleName;
        user.LastName = request.LastName;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            return BadRequest(ApiResponse.ErrorResponse(result.Errors.First().Code, StatusCodes.Status400BadRequest));
        }

        return ApiResponse.SuccessResponse("Name updated successfully.");
    }

    /// <summary>
    /// Updates the dentist-specific profile fields (Specialty and Consultation Fee).
    /// Only accessible by dentists.
    /// </summary>
    [HttpPut("dentist-profile")]
    [Authorize(Roles = RoleNames.Dentist)]
    public async Task<ActionResult<ApiResponse>> UpdateDentistProfile(UpdateDentistProfileDto request)
    {
        var user = await _userManager.FindByIdAsync(CurrentUser.Id);
        if (user is not Dentist dentist)
            return NotFound(ApiResponse.ErrorResponse("Dentist profile not found.", StatusCodes.Status404NotFound));

        dentist.Specialty = request.Specialty;
        dentist.ConsultationFee = request.ConsultationFee;

        var result = await _userManager.UpdateAsync(dentist);

        if (!result.Succeeded)
        {
            return BadRequest(ApiResponse.ErrorResponse(result.Errors.First().Code, StatusCodes.Status400BadRequest));
        }

        _logger.LogInformation("Dentist profile updated | UserId: {UserId} | Specialty: {Specialty} | Fee: {Fee}", CurrentUser.Id, request.Specialty, request.ConsultationFee);

        return ApiResponse.SuccessResponse("Dentist profile updated successfully.");
    }

    /// <summary>
    /// Gets a list of appointments for the currently authenticated dentist.
    /// </summary>
    /// <param name="status">Optional filter: "upcoming" or "completed".</param>
    [HttpGet("my-appointments")]
    [Authorize(Roles = RoleNames.Dentist)]
    public async Task<ActionResult<ApiResponse>> GetDentistAppointments([FromQuery] string? status)
    {
        var query = _context.Appointments
            .Include(a => a.Slot)
            .Include(a => a.Patient)
            .Include(a => a.Payment)
            .Where(a => a.DentistId == CurrentUser.Id);

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
            .Select(a => new DentistAppointmentListItemDto
            {
                Id = a.Id,
                Status = a.Status,
                AppointmentType = a.AppointmentType,
                SlotDate = a.Slot.Date,
                SlotFrom = a.Slot.From,
                SlotTo = a.Slot.To,
                PatientId = a.Patient.Id,
                PatientName = a.Patient.FullName,
                PaymentId = a.Payment != null ? a.Payment.Id : null,
                PaymentStatus = a.Payment != null ? a.Payment.Status : null
            })
            .ToListAsync();

        return ApiResponse.SuccessResponse(appointments);
    }
}
