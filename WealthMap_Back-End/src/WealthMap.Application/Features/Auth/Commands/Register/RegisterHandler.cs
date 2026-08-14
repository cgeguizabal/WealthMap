using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Common.Services;
using WealthMap.Application.Features.Auth.DTOs;
using WealthMap.Domain.Entities;
using WealthMap.Domain.Exceptions;

namespace WealthMap.Application.Features.Auth.Commands.Register;

public class RegisterHandler : ICommandHandler<RegisterCommand, AuthSessionDto>
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly SessionIssuer _sessions;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterHandler(
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

    public async Task<AuthSessionDto> Handle(RegisterCommand request, CancellationToken ct)
    {
        if (await _users.EmailExistsAsync(request.Email, ct))
            throw new DomainException("An account with this email already exists.");

        var passwordHash = _passwordHasher.Hash(request.Password);

        var user = new User(
            request.Email,
            passwordHash,
            request.FullName,
            request.Country,
            request.Currency);

        AuthSessionDto? session = null;

        // The refresh token carries a foreign key to the user, so both rows have to
        // land together or neither should.
        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            await _users.AddAsync(user, ct);
            session = await _sessions.IssueAsync(user, ct);
        }, ct);

        return session!;
    }
}
