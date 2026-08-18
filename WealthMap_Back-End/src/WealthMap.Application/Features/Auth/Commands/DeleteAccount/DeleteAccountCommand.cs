using WealthMap.Application.Common.Messaging;

namespace WealthMap.Application.Features.Auth.Commands.DeleteAccount;

/// <summary>
/// Erases the user and everything they recorded. Immediate and irreversible.
/// </summary>
/// <param name="Password">
/// Re-entered to prove the person at the keyboard is the account holder.
///
/// A valid access token is not enough on its own: tokens live in a browser and
/// outlive the moment they were issued, so a borrowed laptop or a stolen token
/// would be sufficient to destroy someone's records. Every other action in the
/// app can be undone; this one cannot.
/// </param>
public record DeleteAccountCommand(Guid UserId, string Password) : ICommand<bool>;
