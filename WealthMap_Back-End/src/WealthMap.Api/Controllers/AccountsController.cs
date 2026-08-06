using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WealthMap.Api.Extensions;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Accounts.Commands.CreateAccount;
using WealthMap.Application.Features.Accounts.Queries.GetAccounts;


namespace WealthMap.Api.Controllers;

[ApiController]
[Route("api/accounts")]
[Authorize]                                    // ← every action requires a valid token
public class AccountsController : ControllerBase
{
    private readonly ISender _sender;

    public AccountsController(ISender sender) => _sender = sender;

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateAccountRequest request,
        CancellationToken ct)
    {
        var userId = User.GetUserId();         // ← from the JWT. Hardcoded GUID: deleted.

        var command = new CreateAccountCommand(
            userId,
            request.Name,
            request.BankName,
            request.Type,
            request.OpeningBalance,
            request.Currency);

        var result = await _sender.Send(command, ct);

        return Ok(result);    }

      [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _sender.Send(new GetAccountsQuery(User.GetUserId()), ct);
        return Ok(result);
    }
}


public record CreateAccountRequest(
    string Name,
    string BankName,
    int Type,
    decimal OpeningBalance,
    string Currency);