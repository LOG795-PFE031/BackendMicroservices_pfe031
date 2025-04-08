using System.Security.Claims;

namespace Portfolio.Controllers;

internal static class ControllerHelpers
{
    public static string GetUsername(this ClaimsPrincipal user)
    {
        return user.Claims.Single(claim => claim.Type.Equals(ClaimTypes.NameIdentifier)).Value;
    }
}