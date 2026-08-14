namespace WealthMap.Application.Common.Exceptions;

/// <summary>
/// The caller's credentials are missing, expired, or no longer trusted.
/// </summary>
/// <remarks>
/// Distinct from <c>DomainException</c> because the status code is what the
/// frontend acts on: a 400 tells it to show the message, a 401 tells it the
/// session is over and to send the user to the login screen. Refresh failures
/// must be the second kind, or a spent token would surface as a form error.
///
/// The message is deliberately vague at every call site. Saying whether a token
/// was unknown, expired, or already used tells an attacker which of those it is.
/// </remarks>
public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message = "Your session is no longer valid.")
        : base(message) { }
}
