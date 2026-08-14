using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;

namespace WealthMap.Application.Features.Auth.Commands.Logout;

/// <summary>
/// Revokes the presented refresh token, or every token the user has.
/// </summary>
/// <remarks>
/// Never fails. An unknown, expired, or missing token still returns success:
/// the caller wanted the session ended, and it is ended. Reporting an error
/// would only tell someone probing the endpoint which tokens are real, and would
/// leave the frontend deciding whether to clear its state after a failed logout.
/// </remarks>
public class LogoutHandler : ICommandHandler<LogoutCommand, bool>
{
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IRefreshTokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;

    public LogoutHandler(
        IRefreshTokenRepository refreshTokens,
        IRefreshTokenService tokenService,
        IUnitOfWork unitOfWork)
    {
        _refreshTokens = refreshTokens;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(LogoutCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return true;

        var presented = await _refreshTokens.GetByHashAsync(
            _tokenService.Hash(request.RefreshToken), ct);

        if (presented is null)
            return true;

        if (request.AllSessions)
        {
            foreach (var token in await _refreshTokens.GetActiveForUserAsync(presented.UserId, ct))
            {
                token.Revoke();
                _refreshTokens.Update(token);
            }
        }
        else
        {
            presented.Revoke();
            _refreshTokens.Update(presented);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        return true;
    }
}
