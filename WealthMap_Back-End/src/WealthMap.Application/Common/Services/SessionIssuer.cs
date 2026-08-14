using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Features.Auth.DTOs;
using WealthMap.Domain.Entities;

namespace WealthMap.Application.Common.Services;

/// <summary>
/// Issues an access token and a refresh token together. Login, register and
/// refresh all need exactly this, and a session that came out of one of them
/// should be indistinguishable from a session that came out of another.
/// </summary>
/// <remarks>
/// Does not save. The caller owns the transaction, because refresh has to revoke
/// the old token and store the new one as a single unit, while register has a
/// user to persist first.
/// </remarks>
public class SessionIssuer
{
    private readonly IJwtService _jwt;
    private readonly IRefreshTokenService _refreshTokens;
    private readonly IRefreshTokenRepository _repository;

    public SessionIssuer(
        IJwtService jwt,
        IRefreshTokenService refreshTokens,
        IRefreshTokenRepository repository)
    {
        _jwt = jwt;
        _refreshTokens = refreshTokens;
        _repository = repository;
    }

    public async Task<AuthSessionDto> IssueAsync(User user, CancellationToken ct)
    {
        var accessToken = _jwt.GenerateToken(user);

        // The raw value exists here and in the response cookie, and nowhere else.
        var rawRefreshToken = _refreshTokens.GenerateToken();

        var refreshToken = new RefreshToken(
            user.Id,
            _refreshTokens.Hash(rawRefreshToken),
            DateTime.UtcNow.Add(_refreshTokens.Lifetime));

        await _repository.AddAsync(refreshToken, ct);

        return new AuthSessionDto(
            new AuthResultDto(user.Id, user.Email, user.FullName, accessToken),
            rawRefreshToken);
    }
}
