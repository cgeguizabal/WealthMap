using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Auth.DTOs;
using WealthMap.Domain.Entities;
using WealthMap.Domain.Exceptions;

namespace WealthMap.Application.Features.Auth.Commands.Register;

public class RegisterHandler : ICommandHandler<RegisterCommand, AuthResultDto>
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwt;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterHandler(
        IUserRepository users,
        IPasswordHasher passwordHasher,
        IJwtService jwt,
        IUnitOfWork unitOfWork)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _jwt = jwt;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResultDto> Handle(RegisterCommand request, CancellationToken ct)
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

        await _users.AddAsync(user, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var token = _jwt.GenerateToken(user);

        return new AuthResultDto(user.Id, user.Email, user.FullName, token);
    }
}