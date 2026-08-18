using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Common.Services;
using WealthMap.Application.Features.Auth.DTOs;
using WealthMap.Domain.Exceptions;

namespace WealthMap.Application.Features.Auth.Commands.Login;

public class LoginHandler : ICommandHandler<LoginCommand, AuthSessionDto>
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly SessionIssuer _sessions;
    private readonly IUnitOfWork _unitOfWork;

    public LoginHandler(
        IUserRepository users,
        IPasswordHasher passwordHasher,
        SessionIssuer sessions,
        IUnitOfWork unitOfWork)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _sessions = sessions;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthSessionDto> Handle(LoginCommand request, CancellationToken ct)
    {
        var user = await _users.GetByEmailAsync(request.Email, ct);

        if (user is null || !_passwordHasher.Verify(user.PasswordHash, request.Password))
            throw new DomainException("Invalid email or password.");

        var session = await _sessions.IssueAsync(user, ct);

        // Logging in now writes a row, where before it only read.
        await _unitOfWork.SaveChangesAsync(ct);

        return session;
    }
}
