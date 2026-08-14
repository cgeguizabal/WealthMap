namespace WealthMap.Infrastructure.Auth;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Secret { get; init; } = string.Empty;
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public int ExpiryMinutes { get; init; } = 60;

    /// <summary>
    /// How long a refresh token lives. Long enough that a user who opens the app
    /// most weeks is never asked to sign in again; short enough that an abandoned
    /// session eventually closes itself.
    /// </summary>
    public int RefreshTokenDays { get; init; } = 14;
}