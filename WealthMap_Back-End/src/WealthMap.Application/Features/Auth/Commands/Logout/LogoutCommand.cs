using WealthMap.Application.Common.Messaging;

namespace WealthMap.Application.Features.Auth.Commands.Logout;

/// <param name="RefreshToken">From the cookie. Absent or unknown is still a success.</param>
/// <param name="AllSessions">Ends every session for the user, not only this device.</param>
public record LogoutCommand(string? RefreshToken, bool AllSessions = false) : ICommand<bool>;
