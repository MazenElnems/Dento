using AngDepiApi_DentalClinic.Consts;
using AngDepiApi_DentalClinic.DbContexts;
using AngDepiApi_DentalClinic.DTOs;
using AngDepiApi_DentalClinic.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AngDepiApi_DentalClinic.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AccountController : ControllerBase
    {

        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;




        public AccountController(
            IConfiguration configuration,
            UserManager<AppUser> userManager)
        {
            _configuration = configuration;
            _userManager = userManager;
        }



        [HttpPost]
        public async Task<ActionResult> Register(RegisterDTO input)
        {
            try
            {


                if(User.Identity != null && User.Identity.IsAuthenticated)
                    return BadRequest("You are already logged in.");
                if (ModelState.IsValid)
                {



                    var user = new AppUser
                    {
                        UserName = input.Email,
                        Email = input.Email
                    };
                    var result = await _userManager.CreateAsync(user, input.Password!);
                    if (result.Succeeded)
                    {

                        await _userManager.AddToRoleAsync(user, RolesNames.Patient);

                        return StatusCode(StatusCodes.Status201Created, $"User '{user.UserName}' has been created.");
                    }
                        
                    else
                        throw new Exception(
                            string.Format("Error: {0}", string.Join(" ",
                                result.Errors.Select(e => e.Description))));

                }
                else
                {
                    var details = new ValidationProblemDetails(ModelState);
                    details.Type =
                        "https:/ /tools.ietf.org/html/rfc7231#section-6.5.1";
                    details.Status = StatusCodes.Status400BadRequest;
                    return new BadRequestObjectResult(details);
                }
            }
            catch (Exception e)
            {
                var exceptionDetails = new ProblemDetails();
                exceptionDetails.Detail = e.Message;
                exceptionDetails.Status =
                    StatusCodes.Status500InternalServerError;
                exceptionDetails.Type =
                    "https:/ /tools.ietf.org/html/rfc7231#section-6.6.1";
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    exceptionDetails);
            }
        }
        [HttpPost]
        public async Task<ActionResult> Login(LoginDTO input)
        {
            try
            {
                if (User.Identity != null && User.Identity.IsAuthenticated)
                    return BadRequest("You are already logged in.");
                var user = await _userManager.FindByNameAsync(input.Email!);
                if (user == null || !await _userManager.CheckPasswordAsync(user, input.Password!))
                    throw new Exception("Invalid login attempt.");

                else
                {
                    var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(System.Text.Encoding.UTF8
                       .GetBytes(_configuration["JWT:SigningKey"]!)),
                                 SecurityAlgorithms.HmacSha256);



                    var userRoles =await _userManager.GetRolesAsync(user);

                    var claims = new List<Claim>();

                    //claims.Add(new Claim(ClaimTypes.Name, user.UserName!));
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
                    return StatusCode(
                        StatusCodes.Status200OK, new
                        {
                            token = jwtString,
                            expiration = jwtObject.ValidTo
                        });


                }



            }
            catch (Exception e)
            {
                var exceptionDetails = new ProblemDetails();
                exceptionDetails.Detail = e.Message;
                exceptionDetails.Status =
                    StatusCodes.Status401Unauthorized;
                exceptionDetails.Type =
                    "https:/ /tools.ietf.org/html/rfc7231#section-6.6.1";

                return StatusCode(StatusCodes.Status401Unauthorized, exceptionDetails);

            }
        }




    }
}
