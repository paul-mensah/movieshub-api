using System.Security.Claims;
using MoviesHub.Api.Models.Response;
using Newtonsoft.Json;

namespace MoviesHub.Api.Helpers;

public static class UserHelper
{
    public static UserResponse GetUserData(this ClaimsPrincipal claims)
    {
        ClaimsIdentity claimsIdentity = claims.Identities.FirstOrDefault(i => i.AuthenticationType == CommonConstants.Authentication.AppAuthIdentity);
        Claim userData = claimsIdentity?.FindFirst(ClaimTypes.Thumbprint);

        if (userData is null)
        {
            return new UserResponse();
        }

        UserResponse user = JsonConvert.DeserializeObject<UserResponse>(userData.Value);
        return user;
    }
}
