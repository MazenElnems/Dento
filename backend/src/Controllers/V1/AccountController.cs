using Asp.Versioning;
using Azure;
using Dento.Constants;
using Dento.Controllers.Common;
using Dento.Data;
using Dento.DTOs;
using Dento.Exceptions;
using Dento.Models;
using Dento.Options;
using Dento.Services.Interfaces;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;

namespace Dento.Controllers.v1; 

[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion("1.0")]
public class AccountController : BaseApiController
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ClientSettings _clientSettings;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        AppDbContext context,
        ITokenService authService,
        IEmailService emailService,
        IOptions<ClientSettings> clientSettings)
    {
        _userManager = userManager;
        _context = context;
        _tokenService = authService;
        _emailService = emailService;
        _clientSettings = clientSettings.Value;
    }

    /// <summary>
    /// Registers a new patient account.
    /// </summary>
    /// <remarks>
    /// Creates a new patient account and sends a verification code to the user's email.
    /// The email address must be unique.
    /// The caller must not already be authenticated.
    /// </remarks>
    /// <param name="request">Patient registration information.</param>
    /// <returns>The created user's identifier and email address.</returns>
    /// <response code="200">Registration completed successfully. Verification code has been sent.</response>
    /// <response code="400">Invalid request, email already exists, or user is already authenticated.</response>
    /// <response code="500">An unexpected error occurred while creating the account.</response>
    /// 
    [HttpPost("register")]
    [SwaggerOperation( Summary = "Register a new patient", Description = "Creates a patient account and sends an email verification code.")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse>> Register(RegisterPatientRequestDto request)
    {
        if(CurrentUser.IsAuthenticated)
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


    /// <summary>
    /// Verifies a user's email address.
    /// </summary>
    /// <remarks>
    /// Confirms the user's email using the verification code previously sent.
    /// The verification code must be active and not expired.
    /// </remarks>
    /// <param name="request">Verification request.</param>
    /// <returns>A success message.</returns>
    /// <response code="200">Email verified successfully.</response>
    /// <response code="400">Invalid or expired verification code.</response>
    [HttpPost("verify-email")]
    [SwaggerOperation(Summary = "Verify email", Description = "Verifies a user's email using the verification code.")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
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


    /// <summary>
    /// Sends a new email verification code.
    /// </summary>
    /// <remarks>
    /// Invalidates any previously active verification codes before generating a new one.
    /// </remarks>
    /// <param name="request">User email.</param>
    /// <returns>A success message.</returns>
    /// <response code="200">Verification code sent successfully.</response>
    /// <response code="404">No account exists with the specified email.</response>
    [HttpPost("send-verification-code")]
    [SwaggerOperation(Summary = "Resend verification code", Description = "Generates and emails a new verification code.")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
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


    /// <summary>
    /// Sends a password reset email.
    /// </summary>
    /// <remarks>
    /// Generates a password reset token and sends a password reset link to the user's email.
    /// </remarks>
    /// <param name="request">User email.</param>
    /// <returns>A success message.</returns>
    /// <response code="200">Password reset email sent.</response>
    /// <response code="404">No account exists with the specified email.</response>
    [HttpPost("forget-password")]
    [SwaggerOperation(Summary = "Request password reset", Description = "Generates a password reset link and emails it to the user.")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> ForgetPassword(ForgetPasswordRequestDto request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null) 
            return NotFound(ApiResponse.ErrorResponse(ErrorCodes.EmailNotFound, StatusCodes.Status404NotFound));
    
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        var resetPasswordUrl = $"{_clientSettings.Host}/reset-password?userId={user.Id}&token={token}";

        BackgroundJob.Enqueue(() => _emailService.SendResetPasswordEmailAsync(user.FirstName, user.Email!, resetPasswordUrl, 30));

        return ApiResponse.SuccessResponse("Password reset email sent successfully.", StatusCodes.Status200OK);
    }


    /// <summary>
    /// Resets a user's password.
    /// </summary>
    /// <remarks>
    /// Resets the user's password using a valid password reset token.
    /// </remarks>
    /// <param name="request">Password reset information.</param>
    /// <returns>A success message.</returns>
    /// <response code="200">Password reset successfully.</response>
    /// <response code="400">Invalid or expired reset token.</response>
    /// <response code="404">User not found.</response>
    [HttpPost("reset-password")]
    [SwaggerOperation(Summary = "Reset password", Description = "Resets the user's password using the reset token.")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> ResetPassword(ResetPasswordRequestDto request)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);

        if(user == null)
            return NotFound(ApiResponse.ErrorResponse(ErrorCodes.UserNotFound, StatusCodes.Status404NotFound));

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);

        if (!result.Succeeded)
            return BadRequest(ApiResponse.ErrorResponse(result.Errors.First().Code, StatusCodes.Status400BadRequest));

        return ApiResponse.SuccessResponse("Password reset successfully.", StatusCodes.Status200OK);
    }


    /// <summary>
    /// Creates a dentist account.
    /// </summary>
    /// <remarks>
    /// Creates a new dentist user and assigns the Dentist role.
    /// Only administrators can access this endpoint.
    /// </remarks>
    /// <param name="input">Dentist registration information.</param>
    /// <returns>The created dentist.</returns>
    /// <response code="201">Dentist created successfully.</response>
    /// <response code="400">Invalid request or email already exists.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="403">Only administrators can perform this operation.</response>
    [HttpPost("register-dentist")]
    [Authorize(Roles = RoleNames.Admin)]
    [SwaggerOperation(Summary = "Register dentist", Description = "Creates a new dentist account.")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> RegisterDentist(RegisterDentistDto input)
    {
        var existingUser = await _userManager.FindByEmailAsync(input.Email);

        if (existingUser != null)
        {
            throw new BaseException(
                StatusCodes.Status400BadRequest,
                "Email already exists.");
        }

        var dentist = new Dentist
        {
            UserName = input.Email,
            Email = input.Email,
            FirstName = input.FirstName,
            MiddleName = input.MiddleName,
            LastName = input.LastName,
            Specialty = input.Specialty,
            EmailConfirmed = true
        };

        dentist.BuildDefaultSchedule(); // intilize the dentist availability

        var result = await _userManager.CreateAsync(dentist, input.Password!);

        if (!result.Succeeded)
        {
            throw new BaseException(
                StatusCodes.Status400BadRequest,
                string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        await _userManager.AddToRoleAsync(dentist, RoleNames.Dentist);

        await _context.SaveChangesAsync();

        return Created(
            string.Empty,
            ApiResponse.SuccessResponse(
                "Doctor account created successfully.",
                "Doctor created",
                StatusCodes.Status201Created));
    }


    /// <summary>
    /// Creates a receptionist account.
    /// </summary>
    /// <remarks>
    /// Creates a receptionist user and assigns the Receptionist role.
    /// Only administrators can access this endpoint.
    /// </remarks>
    /// <param name="input">Receptionist registration information.</param>
    /// <returns>A success message.</returns>
    /// <response code="200">Receptionist created successfully.</response>
    /// <response code="400">Invalid request or email already exists.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="403">Only administrators can perform this operation.</response>
    [Authorize(Roles = RoleNames.Admin)]
    [HttpPost("register-receptionist")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Register receptionist", Description = "Creates a new receptionist account.")]
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
                Email = input.Email,
                EmailConfirmed = true
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


    /// <summary>
    /// Authenticates a user.
    /// </summary>
    /// <remarks>
    /// Validates the supplied credentials and returns an access token.
    /// If the user's email is not verified, the response indicates that email verification is required instead of returning a token.
    /// The caller must not already be authenticated.
    /// </remarks>
    /// <param name="request">User credentials.</param>
    /// <returns>Authentication token and user information.</returns>
    /// <response code="200">Authentication completed successfully.</response>
    /// <response code="400">User is already authenticated.</response>
    /// <response code="401">Invalid email or password.</response>
    /// <response code="500">The user account is improperly configured.</response>
    [HttpPost("login")]
    [SwaggerOperation(Summary = "Authenticate user", Description = "Authenticates a user and returns a JWT access token.")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse>> Login(LoginRequestDto request)
    {
        if(CurrentUser.IsAuthenticated)
            return BadRequest(ApiResponse.ErrorResponse(ErrorCodes.UserAlreadyLoggedIn, StatusCodes.Status400BadRequest));

        var user = await _userManager.FindByNameAsync(request.Email!);

        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password!))
            return StatusCode(
                StatusCodes.Status401Unauthorized,
                ApiResponse.ErrorResponse(ErrorCodes.InvalidCredentials, StatusCodes.Status401Unauthorized)
            );

        var userRoles = await _userManager.GetRolesAsync(user);

        if (!userRoles.Any())
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResponse.ErrorResponse(ErrorCodes.UnhandledException, StatusCodes.Status500InternalServerError)
            );

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

        return ApiResponse.SuccessResponse(loginResponse, "Login successful.");
    }
}
