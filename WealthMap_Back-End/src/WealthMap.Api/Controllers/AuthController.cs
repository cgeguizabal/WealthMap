using Microsoft.AspNetCore.Mvc;
using WealthMap.Api.Auth;
using WealthMap.Api.Extensions;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Auth.Commands.Login;
using WealthMap.Application.Features.Auth.Commands.Logout;
using WealthMap.Application.Features.Auth.Commands.RefreshSession;
using Microsoft.AspNetCore.Authorization;
using WealthMap.Application.Features.Auth.Commands.DeleteAccount;
using WealthMap.Application.Features.Auth.Commands.Register;
using WealthMap.Application.Features.Auth.DTOs;

namespace WealthMap.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly ISender _sender;
    private readonly RefreshTokenCookie _cookie;

    public AuthController(ISender sender, RefreshTokenCookie cookie)
    {
        _sender = sender;
        _cookie = cookie;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command, CancellationToken ct)
        => IssueSession(await _sender.Send(command, ct));

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken ct)
        => IssueSession(await _sender.Send(command, ct));

    /// <summary>
    /// Trades the refresh cookie for a new access token. Takes no body: the token
    /// arrives as a cookie the browser attaches on its own, which is what allows it
    /// to stay unreadable by JavaScript.
    /// </summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(CancellationToken ct)
    {
        var command = new RefreshSessionCommand(_cookie.Read(Request) ?? string.Empty);

        return IssueSession(await _sender.Send(command, ct));
    }

    /// <summary>
    /// Revokes the session. Always succeeds, so the client can clear its own state
    /// unconditionally rather than deciding what a failed logout means.
    /// </summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromQuery] bool allSessions, CancellationToken ct)
    {
        await _sender.Send(new LogoutCommand(_cookie.Read(Request), allSessions), ct);

        _cookie.Clear(Response);

        return NoContent();
    }

    /// <summary>
    /// Splits the session: the refresh token goes to the cookie, and only the rest
    /// is serialised. The DTO keeps them apart so this is the single place either
    /// could leak into a response body.
    /// </summary>
    private IActionResult IssueSession(AuthSessionDto session)
    {
        _cookie.Write(Response, session.RefreshToken);

        return Ok(session.Result);
    }

    /// <summary>
    /// Deletes the account and everything in it. Immediate and irreversible.
    /// </summary>
    /// <remarks>
    /// Requires the password as well as a valid token: a token lives in a browser
    /// and outlives the moment it was issued, so on its own it would let a
    /// borrowed laptop destroy someone's records. Every other action here can be
    /// undone.
    ///
    /// The refresh cookie is cleared on the way out — the session it names no
    /// longer exists, and leaving it would send the client into a refresh that
    /// could only fail.
    /// </remarks>
    [Authorize]
    [HttpDelete("me")]
    public async Task<IActionResult> DeleteAccount(
        [FromBody] DeleteAccountRequest request,
        CancellationToken ct)
    {
        await _sender.Send(new DeleteAccountCommand(User.GetUserId(), request.Password), ct);

        _cookie.Clear(Response);

        return NoContent();
    }

    public record DeleteAccountRequest(string Password);
}
