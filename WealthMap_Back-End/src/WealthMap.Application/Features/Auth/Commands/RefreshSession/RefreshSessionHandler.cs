using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Common.Services;
using WealthMap.Application.Features.Auth.DTOs;

namespace WealthMap.Application.Features.Auth.Commands.RefreshSession;

/// <summary>
/// Exchanges a refresh token for a new access token and a new refresh token.
/// </summary>
public class RefreshSessionHandler : ICommandHandler<RefreshSessionCommand, AuthSessionDto>
{
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IRefreshTokenService _tokenService;
    private readonly IUserRepository _users;
    private readonly SessionIssuer _sessions;
    private readonly IUnitOfWork _unitOfWork;

    public RefreshSessionHandler(
        IRefreshTokenRepository refreshTokens,
        IRefreshTokenService tokenService,
        IUserRepository users,
        SessionIssuer sessions,
        IUnitOfWork unitOfWork)
    {
        _refreshTokens = refreshTokens;
        _tokenService = tokenService;
        _users = users;
        _sessions = sessions;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthSessionDto> Handle(RefreshSessionCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            throw new UnauthorizedException();

        var presented = await _refreshTokens.GetByHashAsync(
            _tokenService.Hash(request.RefreshToken), ct);

        if (presented is null)
            throw new UnauthorizedException();

        // A revoked token being presented means the value survived somewhere it
        // should not have: this one was already spent, and rotation means the
        // legitimate client has since moved on to a different value. Either it was
        // stolen and is being replayed, or the real client is replaying it — and
        // there is no way to tell which. Ending every session is the safe reading,
        // and it is also what makes the theft visible: the real user gets logged
        // out and knows something happened.
        if (presented.IsRevoked)
        {
            await RevokeEverythingForAsync(presented.UserId, ct);
            throw new UnauthorizedException();
        }

        if (presented.IsExpired)
            throw new UnauthorizedException();

        var user = await _users.GetByIdAsync(presented.UserId, ct);

        // The account was deleted while a valid token was still circulating.
        if (user is null)
            throw new UnauthorizedException();

        AuthSessionDto? session = null;

        // Spending the old token and storing its replacement is one change. If the
        // second half failed on its own, the client would be holding a token the
        // database had already marked as used, and the next refresh would read as
        // a replay and kill every session.
        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            session = await _sessions.IssueAsync(user, ct);
            presented.ReplaceWith(_tokenService.Hash(session.RefreshToken));
            _refreshTokens.Update(presented);
        }, ct);

        return session!;
    }

    private async Task RevokeEverythingForAsync(Guid userId, CancellationToken ct)
    {
        var active = await _refreshTokens.GetActiveForUserAsync(userId, ct);

        foreach (var token in active)
        {
            token.Revoke();
            _refreshTokens.Update(token);
        }

        await _unitOfWork.SaveChangesAsync(ct);
    }
}
