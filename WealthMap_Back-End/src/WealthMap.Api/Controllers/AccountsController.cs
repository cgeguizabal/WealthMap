using Microsoft.AspNetCore.Mvc;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Accounts.Commands.CreateAccount;

namespace WealthMap.Api.Controllers;

[ApiController]
[Route("api/accounts")]
public class AccountsController : ControllerBase
{
    private readonly ISender _sender;

    public AccountsController(ISender sender) => _sender = sender;

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateAccountRequest request,
        CancellationToken ct)
    {
        // TEMPORARY: hardcoded until JWT auth (Phase 6) supplies the real user id
        var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        var command = new CreateAccountCommand(
            userId,
            request.Name,
            request.BankName,
            request.Type,
            request.OpeningBalance,
            request.Currency);

        var result = await _sender.Send(command, ct);

        return CreatedAtAction(nameof(Create), new { id = result.Id }, result);
    }
}

public record CreateAccountRequest(
    string Name,
    string BankName,
    int Type,
    decimal OpeningBalance,
    string Currency);