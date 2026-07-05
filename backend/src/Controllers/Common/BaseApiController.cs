using Dento.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Dento.Controllers.Common;

[ApiController]
public class BaseApiController : ControllerBase
{
    protected CurrentUser GetCurrentUser()
    {
        if (User.Identity == null || !User.Identity.IsAuthenticated)
        {
            return CurrentUser.Unauthenticated;
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? 
            throw new InvalidOperationException("User ID not found");

        var userEmail = User.FindFirstValue(ClaimTypes.Email) ?? 
            throw new InvalidOperationException("User email not found");

        var userRole = User.FindFirstValue(ClaimTypes.Role) ?? 
            throw new InvalidOperationException("User role not found");

        return CurrentUser.Authenticated(userId, userEmail, userRole);
    }
}
