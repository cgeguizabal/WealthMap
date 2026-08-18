namespace WealthMap.Api.Auth;

/// <summary>
/// How the refresh cookie is scoped. Configuration rather than constants because
/// the right answer depends on where the frontend is served from.
/// </summary>
/// <remarks>
/// Same-origin deployments (frontend and API behind one host, or Vite's dev proxy)
/// want <c>SameSite=Lax</c>, which is the stricter of the two and blocks the cookie
/// on cross-site requests entirely.
///
/// A frontend on a different origin must use <c>SameSite=None</c>, and browsers only
/// accept that together with <c>Secure</c> — so that combination requires HTTPS and
/// the CORS policy must also allow credentials.
/// </remarks>
public class CookieSettings
{
    public const string SectionName = "Auth:Cookie";

    /// <summary>"Lax", "Strict", or "None".</summary>
    public string SameSite { get; init; } = "Lax";

    /// <summary>False only for local HTTP development; a cookie over plain HTTP is readable in transit.</summary>
    public bool Secure { get; init; } = true;

    public SameSiteMode SameSiteMode => SameSite.ToLowerInvariant() switch
    {
        "none" => SameSiteMode.None,
        "strict" => SameSiteMode.Strict,
        _ => SameSiteMode.Lax
    };
}
