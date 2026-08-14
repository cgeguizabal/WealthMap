using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using WealthMap.Application.Common.Interfaces;

namespace WealthMap.Infrastructure.Auth;

public class RefreshTokenService : IRefreshTokenService
{
    // 256 bits. Far past guessing, and it keeps the cookie a reasonable size.
    private const int TokenBytes = 32;

    private readonly JwtSettings _settings;

    public RefreshTokenService(IOptions<JwtSettings> options) => _settings = options.Value;

    public TimeSpan Lifetime => TimeSpan.FromDays(_settings.RefreshTokenDays);

    /// <summary>
    /// <c>RandomNumberGenerator</c>, not <c>Random</c> or <c>Guid</c>: this value is
    /// a credential, so it has to be unpredictable rather than merely unique.
    /// Base64Url keeps it safe to put in a cookie without escaping.
    /// </summary>
    public string GenerateToken() =>
        Base64UrlEncode(RandomNumberGenerator.GetBytes(TokenBytes));

    public string Hash(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
}
