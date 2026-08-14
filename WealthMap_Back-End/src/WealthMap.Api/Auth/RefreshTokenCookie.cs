using Microsoft.Extensions.Options;
using WealthMap.Infrastructure.Auth;

namespace WealthMap.Api.Auth;

/// <summary>
/// Reads and writes the refresh token cookie. One place, so the security flags
/// cannot drift between the endpoints that set it and the one that clears it.
/// </summary>
public class RefreshTokenCookie
{
    public const string Name = "wm_refresh";

    private readonly CookieSettings _settings;
    private readonly JwtSettings _jwt;

    public RefreshTokenCookie(IOptions<CookieSettings> settings, IOptions<JwtSettings> jwt)
    {
        _settings = settings.Value;
        _jwt = jwt.Value;
    }

    public string? Read(HttpRequest request) => request.Cookies[Name];

    public void Write(HttpResponse response, string token) =>
        response.Cookies.Append(Name, token, BuildOptions(DateTimeOffset.UtcNow.AddDays(_jwt.RefreshTokenDays)));

    /// <summary>
    /// Clearing has to repeat the same Path, Secure, and SameSite values used when
    /// writing. A browser treats a cookie with different attributes as a different
    /// cookie and will leave the original in place.
    /// </summary>
    public void Clear(HttpResponse response) =>
        response.Cookies.Append(Name, string.Empty, BuildOptions(DateTimeOffset.UnixEpoch));

    private CookieOptions BuildOptions(DateTimeOffset expires) => new()
    {
        // The entire point: script on the page cannot read this value, so an XSS
        // can act as the user only while the page is open, and cannot walk away
        // with a two-week credential.
        HttpOnly = true,

        Secure = _settings.Secure,
        SameSite = _settings.SameSiteMode,

        // Scoped to the only endpoints that consume it, so it is not attached to
        // every ordinary API call for no reason.
        Path = "/api/v1/auth",

        Expires = expires
    };
}
