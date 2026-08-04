using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Auth.DTOs;

namespace WealthMap.Application.Features.Auth.Commands.Register;

public record RegisterCommand(
    string Email,
    string Password,
    string FullName,
    string Country,
    string Currency) : ICommand<AuthResultDto>;