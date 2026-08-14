using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Auth.DTOs;

namespace WealthMap.Application.Features.Auth.Commands.RefreshSession;

/// <summary>
/// The token comes from the httpOnly cookie, read by the controller — never from
/// a request body, or JavaScript would have to be able to read it.
/// </summary>
public record RefreshSessionCommand(string RefreshToken) : ICommand<AuthSessionDto>;
