using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Domain.Exceptions;

namespace WealthMap.Application.Features.Users.Commands.ChangePassword;

public class ChangePasswordHandler : ICommandHandler<ChangePasswordCommand, bool>
{
    private readonly IUserRepository _users;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public ChangePasswordHandler(
        IUserRepository users,
        IRefreshTokenRepository refreshTokens,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork)
    {
        _users = users;
        _refreshTokens = refreshTokens;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(ChangePasswordCommand request, CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(request.UserId, ct)
            ?? throw new NotFoundException("User", request.UserId);

        if (!_passwordHasher.Verify(user.PasswordHash, request.CurrentPassword))
            throw new DomainException("Current password is incorrect.");

        if (_passwordHasher.Verify(user.PasswordHash, request.NewPassword))
            throw new DomainException("The new password must be different from the current one.");

        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            user.ChangePassword(_passwordHasher.Hash(request.NewPassword));

            // Every session ends, including this one.
            //
            // A password is usually changed because someone believes a session has
            // been taken. Leaving the other sessions alive would change the lock
            // and hand the intruder a key that still works — the refresh token
            // they hold does not care what the password is.
            //
            // Signing the user out of their own browser too is the cost of that,
            // and it is the right side of the trade: one re-login against an
            // attacker who keeps access for two weeks.
            foreach (var token in await _refreshTokens.GetActiveForUserAsync(request.UserId, ct))
                token.Revoke();
        }, ct);

        return true;
    }
}
