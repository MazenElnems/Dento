using Dento.Constants;
using Dento.Controllers.Common;
using Dento.Data;
using Dento.DTOs;
using Dento.Exceptions;
using Dento.Models;
using Dento.Services.Interfaces;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dento.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountController : BaseApiController
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        AppDbContext context,
        ITokenService authService,
        IEmailService emailService)
    {
        _userManager = userManager;
        _context = context;
        _tokenService = authService;
        _emailService = emailService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse>> Register(RegisterPatientRequestDto request)
    {
        var currentUser = GetCurrentUser();
        
        if(currentUser.IsAuthenticated)
            return BadRequest(ApiResponse.ErrorResponse(ErrorCodes.UserAlreadyLoggedIn, StatusCodes.Status400BadRequest));
        
        var existingUser = await _userManager.FindByEmailAsync(request.Email);

        if (existingUser != null)
            return BadRequest(ApiResponse.ErrorResponse(ErrorCodes.EmailAlreadyExists, StatusCodes.Status400BadRequest));

        var patient = new Patient
        {
            FirstName = request.FirstName,
            MiddleName = request.MiddleName,
            LastName = request.LastName,
            PhoneNumber = request.Phone,
            DateOfBirth = request.BirthDate,
            Email = request.Email,
            UserName = request.Email
        };
        
        var result = await _userManager.CreateAsync(patient, request.Password);

        if(!result.Succeeded)
            return BadRequest(ApiResponse.ErrorResponse(result.Errors.First().Code, StatusCodes.Status400BadRequest));

        var roleResult = await _userManager.AddToRoleAsync(patient, RoleNames.Patient);

        if (!roleResult.Succeeded)
        {
            // log warning: user created but role assignment failed
            await _userManager.DeleteAsync(patient); // rollback user creation
            
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResponse.ErrorResponse(ErrorCodes.UnhandledException, StatusCodes.Status500InternalServerError)
            );
        }

        var code = Random.Shared.Next(100000, 999999).ToString(); // Generate a 6-digit code

        var emailVerificationCode = new EmailVerificationCode
        {
            Code = code,
            ExpiresAt = DateTime.UtcNow.AddMinutes(30), 
            UserId = patient.Id,
            IsActive = true
        };

        _context.EmailVerificationCodes.Add(emailVerificationCode);
        await _context.SaveChangesAsync();

        BackgroundJob.Enqueue(() => _emailService.SendVerificationEmailAsync(patient.FirstName, patient.Email, code, 30));

        var patientDto = new RegisterResponseDto
        {
            UserId = patient.Id,
            Email = patient.Email
        };

        return Ok(ApiResponse.SuccessResponse(patientDto));
    }

    [HttpPost("verify-email")]
    public async Task<ActionResult<ApiResponse>> VerifyEmail(VerifyEmailRequestDto request)
    {
        var verificationCode = await _context
            .EmailVerificationCodes
            .Include(c => c.User)
            .FirstOrDefaultAsync(vc => vc.UserId == request.UserId && vc.Code == request.Code && vc.IsActive);

        if(verificationCode == null || verificationCode.ExpiresAt < DateTime.UtcNow)
            return BadRequest(ApiResponse.ErrorResponse(ErrorCodes.InvalidVerificationCode, StatusCodes.Status400BadRequest));

        var user = verificationCode.User;

        user.EmailConfirmed = true;
        verificationCode.IsActive = false;

        await _context.SaveChangesAsync();

        return ApiResponse.SuccessResponse("Email verified successfully.", StatusCodes.Status200OK);
    }

    [HttpPost("send-verification-code")]
    public async Task<ActionResult<ApiResponse>> SendVerificationCode(SendVerificationCodeRequestDto request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if(user == null)
            return NotFound(ApiResponse.ErrorResponse(ErrorCodes.EmailNotFound, StatusCodes.Status404NotFound));

        var code = Random.Shared.Next(100000, 999999).ToString(); // Generate a 6-digit code

        var emailVerificationCode = new EmailVerificationCode
        {
            Code = code,
            ExpiresAt = DateTime.UtcNow.AddMinutes(30),
            UserId = user.Id,
            IsActive = true
        };

        // deactivate any existing active codes for this user
        await _context.EmailVerificationCodes
            .Where(vc => vc.UserId == user.Id && vc.IsActive)
            .ExecuteUpdateAsync(vc => vc.SetProperty(v => v.IsActive, false));

        _context.EmailVerificationCodes.Add(emailVerificationCode);
        await _context.SaveChangesAsync();

        BackgroundJob.Enqueue(() => _emailService.SendVerificationEmailAsync(user.FirstName, user.Email!, code, 30));
        return ApiResponse.SuccessResponse("Verification code sent successfully.", StatusCodes.Status200OK);
    }

    [HttpPost("register-dentist")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult<ApiResponse>> RegisterDoctor(RegisterDoctorDto input)
    {
        var existingUser = await _userManager.FindByEmailAsync(input.Email);

        if (existingUser != null)
        {
            throw new BaseException(
                StatusCodes.Status400BadRequest,
                "Email already exists.");
        }

        var user = new ApplicationUser
        {
            UserName = input.Email,
            Email = input.Email,
        };

        var result = await _userManager.CreateAsync(user, input.Password!);

        if (!result.Succeeded)
        {
            throw new BaseException(
                StatusCodes.Status400BadRequest,
                string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        await _userManager.AddToRoleAsync(user, RoleNames.Dentist);

        var doctor = new Dentist
        {
            Specialty = input.Specialty,
        };

        _context.Dentists.Add(doctor);

        await _context.SaveChangesAsync();

        return Created(
            string.Empty,
            ApiResponse.SuccessResponse(
                "Doctor account created successfully.",
                "Doctor created",
                StatusCodes.Status201Created));
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPost("register-receptionist")]
    public async Task<ActionResult<ApiResponse>> RegisterReceptionist(RegisterPatientRequestDto input)
    {
        if (User.Identity != null && User.Identity.IsAuthenticated)
            return ApiResponse.ErrorResponse("You are already logged in.", StatusCodes.Status400BadRequest);

        if (ModelState.IsValid)
        {

            var existingUser = await _userManager.FindByEmailAsync(input.Email!);

            if (existingUser != null)
            {
                throw new BaseException(
                    StatusCodes.Status400BadRequest,
                    "Email already exists.");
            }
            var user = new ApplicationUser
            {
                UserName = input.Email,
                Email = input.Email
            };
            var result = await _userManager.CreateAsync(user, input.Password!);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, RoleNames.Receptionist);
                return Ok(ApiResponse.SuccessResponse($"User '{user.UserName}' has been created."));
            }
            else
                throw new BaseException(StatusCodes.Status500InternalServerError,
                    string.Format("Error: {0}", string.Join(" ",
                        result.Errors.Select(e => e.Description))));
        }
        else
        {
            var details = new ValidationProblemDetails(ModelState);
            details.Type =
                "https:/ /tools.ietf.org/html/rfc7231#section-6.5.1";
            details.Status = StatusCodes.Status400BadRequest;
            return ApiResponse.ErrorResponse(string.Join(" ",
                        details.Errors.Select(e => e.Value)), StatusCodes.Status400BadRequest, "Validation failed.");
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse>> Login(LoginRequestDto request)
    {
        var currentUser = GetCurrentUser();

        if(currentUser.IsAuthenticated)
            return ApiResponse.ErrorResponse(ErrorCodes.UserAlreadyLoggedIn, StatusCodes.Status400BadRequest);

        var user = await _userManager.FindByNameAsync(request.Email!);

        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password!))
            return StatusCode(
                StatusCodes.Status401Unauthorized,
                ApiResponse.ErrorResponse(ErrorCodes.InvalidCredentials, StatusCodes.Status401Unauthorized)
            );

        var userRoles = await _userManager.GetRolesAsync(user);

        if(!userRoles.Any())
            return ApiResponse.ErrorResponse(ErrorCodes.UnhandledException, StatusCodes.Status500InternalServerError);

        if (!user.EmailConfirmed)
            return ApiResponse.SuccessResponse(new { UserId = user.Id, user.Email, RequireEmailVerification = true }, "Email not confirmed.");

        var role = userRoles.First();

        var accessToken = await _tokenService.GetAccessToken(user, role);

        var loginResponse = new LoginResponseDto
        {
            Email = user.Email!,
            Role = role,
            Token = accessToken.Token,
            Expiration = accessToken.ExpirationDate
        };

        return Ok(ApiResponse.SuccessResponse(loginResponse, "Login successful."));
    }
}
