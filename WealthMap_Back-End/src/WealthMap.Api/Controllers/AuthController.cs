using Microsoft.AspNetCore.Mvc;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Auth.Commands.Login;
using WealthMap.Application.Features.Auth.Commands.Register;

namespace WealthMap.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender) => _sender = sender;

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command, CancellationToken ct)
        => Ok(await _sender.Send(command, ct));

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken ct)
        => Ok(await _sender.Send(command, ct));
}