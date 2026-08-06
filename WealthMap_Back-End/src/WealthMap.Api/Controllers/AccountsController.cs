using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WealthMap.Api.Extensions;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Accounts.Commands.CreateAccount;
using WealthMap.Application.Features.Accounts.Queries.GetAccounts;
using WealthMap.Application.Features.Accounts.Queries.GetAccountById;
using WealthMap.Application.Features.Accounts.Commands.BlockAccount;
using WealthMap.Application.Features.Accounts.Commands.UnblockAccount;
using WealthMap.Application.Features.Accounts.Commands.UpdateAccount;



namespace WealthMap.Api.Controllers;

[ApiController]
[Route("api/v1/accounts")]
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

return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);          }

      [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _sender.Send(new GetAccountsQuery(User.GetUserId()), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new GetAccountByIdQuery(id, User.GetUserId()), ct);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateAccountRequest request,
        CancellationToken ct)
    {
        var command = new UpdateAccountCommand(
            id,
            User.GetUserId(),
            request.Name,
            request.BankName,
            request.Notes);

        var result = await _sender.Send(command, ct);
        return Ok(result);
    }

    [HttpPost("{id:guid}/block")]
    public async Task<IActionResult> Block(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new BlockAccountCommand(id, User.GetUserId()), ct);
        return Ok(result);
    }

    [HttpPost("{id:guid}/unblock")]
    public async Task<IActionResult> Unblock(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new UnblockAccountCommand(id, User.GetUserId()), ct);
        return Ok(result);
    }
}


public record CreateAccountRequest(
    string Name,
    string BankName,
    int Type,
    decimal OpeningBalance,
    string Currency);

public record UpdateAccountRequest(
    string Name,
    string BankName,
    string? Notes);