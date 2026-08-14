using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Auth.DTOs;

namespace WealthMap.Application.Features.Auth.Commands.Login;

public record LoginCommand(string Email, string Password) : ICommand<AuthSessionDto>;