using System.Security.Claims;

namespace WealthMap.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? principal.FindFirstValue("sub");

        return Guid.TryParse(value, out var id)
            ? id
            : throw new InvalidOperationException("Token does not contain a valid user id.");
    }
}