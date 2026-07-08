using Dento.Models;

namespace Dento.Services.Interfaces;

public interface ITokenService
{
    Task<AccessToken> GetAccessToken(ApplicationUser user, string role);


}
