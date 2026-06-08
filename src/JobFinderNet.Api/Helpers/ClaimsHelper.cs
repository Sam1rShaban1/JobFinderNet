using System.Security.Claims;

namespace JobFinderNet.Api.Helpers;

public static class ClaimsHelper
{
    public static string? GetUserId(this ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue("sub");
    }

    public static List<string> GetRoles(this ClaimsPrincipal user)
    {
        var claimRoles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        var customRoles = user.FindAll("roles").Select(c => c.Value).ToList();
        return claimRoles.Concat(customRoles).Distinct().ToList();
    }

    public static bool HasRole(this ClaimsPrincipal user, string role)
    {
        return user.GetRoles().Any(r =>
            string.Equals(r, role, StringComparison.OrdinalIgnoreCase));
    }

    public static bool HasAnyRole(this ClaimsPrincipal user, params string[] roles)
    {
        var userRoles = user.GetRoles();
        return roles.Any(r => userRoles.Any(ur =>
            string.Equals(ur, r, StringComparison.OrdinalIgnoreCase)));
    }
}
