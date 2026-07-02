using Dento.Constants;
using Dento.Controllers.Common;
using Dento.Data;
using Dento.Data.Entities;
using Dento.DTOs;
using Dento.Exceptions;
using Dento.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Dento.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class AccountController : BaseApiController
{

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _context;




    public AccountController(
        IConfiguration configuration,
        UserManager<ApplicationUser> userManager,
        AppDbContext context)
    {
        _configuration = configuration;
        _userManager = userManager;
        _context = context;
    }



    [HttpPost]
    public async Task<ActionResult<ApiResponse<string>>> Register(RegisterDTO input)
    {
        if (User.Identity != null && User.Identity.IsAuthenticated)
            return ApiResponse<string>.ErrorResponse("You are already logged in.", StatusCodes.Status400BadRequest, "Register failed.");
        
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
                        await _userManager.AddToRoleAsync(user, RoleNames.Patient);
                        return Ok(ApiResponse<string>.SuccessResponse($"User '{user.UserName}' has been created."));
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
                return ApiResponse<string>.ErrorResponse( string.Join(" ",
                            details.Errors.Select(e => e.Value)), StatusCodes.Status400BadRequest, "Validation failed.");
            }
    }




    [Authorize(Roles = RoleNames.Admin)]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<string>>> RegisterDoctor(RegisterDoctorDto input)
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

        await _userManager.AddToRoleAsync(user, RoleNames.Doctor);

        var doctor = new Doctor
        {
            ApplicationUserId = user.Id,
            Specialty = input.Specialty,
        };

        _context.Doctors.Add(doctor);

        await _context.SaveChangesAsync();

        return Created(
            string.Empty,
            ApiResponse<string>.SuccessResponse(
                "Doctor account created successfully.",
                "Doctor created",
                StatusCodes.Status201Created));
    }





    [HttpPost]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult<ApiResponse<string>>> RegisterReceptionist(RegisterDTO input)
    {
        if (User.Identity != null && User.Identity.IsAuthenticated)
            return ApiResponse<string>.ErrorResponse("You are already logged in.", StatusCodes.Status400BadRequest, "Register failed.");

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
                return Ok(ApiResponse<string>.SuccessResponse($"User '{user.UserName}' has been created."));
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
            return ApiResponse<string>.ErrorResponse(string.Join(" ",
                        details.Errors.Select(e => e.Value)), StatusCodes.Status400BadRequest, "Validation failed.");
        }
    }












    [HttpPost]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login(LoginDTO input)
    {
        
            if (User.Identity != null && User.Identity.IsAuthenticated)
                return ApiResponse<LoginResponseDto>.ErrorResponse("You are already logged in.", StatusCodes.Status400BadRequest, "Login failed.");
            var user = await _userManager.FindByNameAsync(input.Email!);
            if (user == null || !await _userManager.CheckPasswordAsync(user, input.Password!))
                throw new BaseException(
                            StatusCodes.Status401Unauthorized,
                            "Invalid email or password.");

            else
            {
                var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(System.Text.Encoding.UTF8
                   .GetBytes(_configuration["JWT:SigningKey"]!)),
                             SecurityAlgorithms.HmacSha256);
                var userRoles = await _userManager.GetRolesAsync(user);
                var claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
                foreach (var role in userRoles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                var jwtObject = new JwtSecurityToken(
                issuer: _configuration["JWT:Issuer"],
                audience: _configuration["JWT:Audience"],
                claims: claims,
                expires: DateTime.Now.AddSeconds(300),
                signingCredentials: signingCredentials);

                var jwtString = new JwtSecurityTokenHandler()
               .WriteToken(jwtObject);
                var response = new LoginResponseDto
                {
                    Token = jwtString,
                    Expiration = jwtObject.ValidTo
                };

                return ApiResponse<LoginResponseDto>.SuccessResponse(
                    response,
                    "Login successful");
            }
    }

















}
