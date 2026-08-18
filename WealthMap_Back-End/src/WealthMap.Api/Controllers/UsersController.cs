using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WealthMap.Api.Auth;
using WealthMap.Api.Extensions;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Users.Commands.ChangePassword;
using WealthMap.Application.Features.Users.Commands.UpdateProfile;
using WealthMap.Application.Features.Users.Queries.GetProfile;

namespace WealthMap.Api.Controllers;

/// <summary>
/// The signed-in user's own account. Every route reads the id from the token, so
/// there is no route that can name someone else.
/// </summary>
[ApiController]
[Route("api/v1/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly ISender _sender;
    private readonly RefreshTokenCookie _cookie;

    public UsersController(ISender sender, RefreshTokenCookie cookie)
    {
        _sender = sender;
        _cookie = cookie;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMe(CancellationToken ct)
        => Ok(await _sender.Send(new GetProfileQuery(User.GetUserId()), ct));

    /// <summary>
    /// Name, country and reporting currency. Email is not editable here — it
    /// identifies the account and carries a unique blind index.
    /// </summary>
    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe(
        [FromBody] UpdateProfileRequest request,
        CancellationToken ct)
    {
        var command = new UpdateProfileCommand(
            User.GetUserId(), request.FullName, request.Country, request.Currency);

        return Ok(await _sender.Send(command, ct));
    }

    /// <summary>
    /// Replaces the password and ends every session, this one included.
    /// </summary>
    /// <remarks>
    /// The refresh cookie is cleared on the way out because the token it names has
    /// just been revoked. Leaving it would send the client into a refresh that
    /// could only fail, and read as a bug rather than as the sign-out it is.
    /// </remarks>
    [HttpPost("me/password")]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken ct)
    {
        var command = new ChangePasswordCommand(
            User.GetUserId(), request.CurrentPassword, request.NewPassword);

        await _sender.Send(command, ct);

        _cookie.Clear(Response);

        return NoContent();
    }

    public record UpdateProfileRequest(string FullName, string Country, string Currency);

    public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
}
