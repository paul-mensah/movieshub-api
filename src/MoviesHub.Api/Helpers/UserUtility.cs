using System.Security.Claims;
using MoviesHub.Api.Models.Response;
using Newtonsoft.Json;

namespace MoviesHub.Api.Helpers;

public static class UserHelper
{
    public static UserResponse GetUserData(this ClaimsPrincipal claims)
    {
        var claimsIdentity = claims.Identities.FirstOrDefault(i => i.AuthenticationType == CommonConstants.AppAuthIdentity);
        var userData = claimsIdentity?.FindFirst(ClaimTypes.Thumbprint);

        if (userData is null)
        {
            return new UserResponse();
        }

        var user = JsonConvert.DeserializeObject<UserResponse>(userData.Value);
        return user;
    }
}
