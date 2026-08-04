using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Auth.DTOs;
using WealthMap.Domain.Exceptions;

namespace WealthMap.Application.Features.Auth.Commands.Login;

public class LoginHandler : ICommandHandler<LoginCommand, AuthResultDto>
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwt;

    public LoginHandler(IUserRepository users, IPasswordHasher passwordHasher, IJwtService jwt)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _jwt = jwt;
    }

    public async Task<AuthResultDto> Handle(LoginCommand request, CancellationToken ct)
    {
        var user = await _users.GetByEmailAsync(request.Email, ct);

        if (user is null || !_passwordHasher.Verify(user.PasswordHash, request.Password))
            throw new DomainException("Invalid email or password.");

        var token = _jwt.GenerateToken(user);

        return new AuthResultDto(user.Id, user.Email, user.FullName, token);
    }
}