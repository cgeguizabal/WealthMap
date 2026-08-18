using WealthMap.Application.Common.Messaging;

namespace WealthMap.Application.Features.Users.Commands.ChangePassword;

/// <summary>
/// Replaces the password and ends every session.
/// </summary>
/// <param name="CurrentPassword">
/// Proves the person changing it is the account holder rather than whoever picked
/// up an unlocked laptop. A valid token is not enough: the whole reason someone
/// changes a password in a hurry is that they think a token has been taken.
/// </param>
public record ChangePasswordCommand(
    Guid UserId,
    string CurrentPassword,
    string NewPassword) : ICommand<bool>;
