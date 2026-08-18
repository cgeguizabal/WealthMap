using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WealthMap.Api.Extensions;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Accounts.Commands.CreateAccount;
using WealthMap.Application.Features.Accounts.Queries.GetAccounts;
using WealthMap.Application.Features.Accounts.Queries.GetAccountById;
using WealthMap.Application.Features.Accounts.Commands.BlockAccount;
using WealthMap.Application.Features.Accounts.Commands.UnblockAccount;
using WealthMap.Application.Features.Accounts.Commands.RestoreAccount;
using WealthMap.Application.Features.Accounts.Commands.UpdateAccount;
using WealthMap.Application.Features.Accounts.Commands.UpdateAccountTracking;
using WealthMap.Application.Features.Accounts.Commands.UpdateAccountDebitCard;
using WealthMap.Application.Features.Accounts.Commands.ArchiveAccount;
using WealthMap.Application.Features.Accounts.Commands.DepositToAccount;
using WealthMap.Application.Features.Accounts.Commands.WithdrawFromAccount;
using WealthMap.Application.Features.Accounts.Commands.TransferBetweenAccounts;
using WealthMap.Application.Features.Accounts.Queries.GetAccountMovements;



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
            request.Currency,
            request.LastFour,
            request.TrackingMode,
            request.DebitCardType,
            request.DebitCardLastFour);

        var result = await _sender.Send(command, ct);

return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);          }

    /// <param name="includeArchived">
    /// Archived accounts are hidden from every list and total by default. Ask for
    /// them to offer restoring one.
    /// </param>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken ct,
        [FromQuery] bool includeArchived = false)
    {
        var query = new GetAccountsQuery(User.GetUserId(), includeArchived);

        return Ok(await _sender.Send(query, ct));
    }

    /// <summary>Brings an archived account back into the lists and totals.</summary>
    [HttpPost("{id:guid}/restore")]
    public async Task<IActionResult> Restore(Guid id, CancellationToken ct)
    {
        await _sender.Send(new RestoreAccountCommand(id, User.GetUserId()), ct);
        return NoContent();
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

    /// <summary>
    /// Archives the account: it leaves the user's lists and totals, but its
    /// movements and every purchase or payment that references it are preserved.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _sender.Send(new ArchiveAccountCommand(id, User.GetUserId()), ct);
        return NoContent();
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

    /// <summary>
    /// Sets the identifying digits and the tracking mode. Nothing consumes them
    /// yet — see "Planned: automatic transaction sync" in docs/PROJECT_GUIDE.md.
    /// </summary>
    [HttpPut("{id:guid}/tracking")]
    public async Task<IActionResult> UpdateTracking(
        Guid id,
        [FromBody] UpdateTrackingRequest request,
        CancellationToken ct)
    {
        var command = new UpdateAccountTrackingCommand(
            id, User.GetUserId(), request.TrackingMode, request.LastFour);

        return Ok(await _sender.Send(command, ct));
    }

    /// <summary>Whether a debit card reaches this account, and its digits.</summary>
    [HttpPut("{id:guid}/debit-card")]
    public async Task<IActionResult> UpdateDebitCard(
        Guid id,
        [FromBody] UpdateDebitCardRequest request,
        CancellationToken ct)
    {
        var command = new UpdateAccountDebitCardCommand(
            id, User.GetUserId(), request.DebitCardType, request.DebitCardLastFour);

        return Ok(await _sender.Send(command, ct));
    }

    [HttpPost("{id:guid}/deposit")]
    public async Task<IActionResult> Deposit(
        Guid id,
        [FromBody] DepositRequest request,
        CancellationToken ct)
    {
        var command = new DepositToAccountCommand(
            id,
            User.GetUserId(),
            request.Amount,
            request.Description,
            request.Type);

        var result = await _sender.Send(command, ct);
        return Ok(result);
    }

    [HttpPost("{id:guid}/withdraw")]
    public async Task<IActionResult> Withdraw(
        Guid id,
        [FromBody] WithdrawRequest request,
        CancellationToken ct)
    {
        var command = new WithdrawFromAccountCommand(
            id,
            User.GetUserId(),
            request.Amount,
            request.Description,
            request.Location);

        var result = await _sender.Send(command, ct);
        return Ok(result);
    }

    [HttpPost("transfer")]
    public async Task<IActionResult> Transfer(
        [FromBody] TransferRequest request,
        CancellationToken ct)
    {
        var command = new TransferBetweenAccountsCommand(
            User.GetUserId(),
            request.FromAccountId,
            request.ToAccountId,
            request.Amount,
            request.Description);

        var result = await _sender.Send(command, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}/movements")]
    public async Task<IActionResult> GetMovements(
        Guid id,
        CancellationToken ct,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetAccountMovementsQuery(id, User.GetUserId(), page, pageSize);
        var result = await _sender.Send(query, ct);
        return Ok(result);
    }
}


public record CreateAccountRequest(
    string Name,
    string BankName,
    int Type,
    decimal OpeningBalance,
    string Currency,
    string? LastFour = null,
    int? TrackingMode = null,
    int? DebitCardType = null,
    string? DebitCardLastFour = null);

/// <summary>Both tracking fields at once — they constrain each other.</summary>
public record UpdateTrackingRequest(
    int TrackingMode,
    string? LastFour);

/// <summary>Type governs digits: None clears them.</summary>
public record UpdateDebitCardRequest(
    int DebitCardType,
    string? DebitCardLastFour);

public record UpdateAccountRequest(
    string Name,
    string BankName,
    string? Notes);

public record DepositRequest(
    decimal Amount,
    string Description,
    int Type);

public record WithdrawRequest(
    decimal Amount,
    string Description,
    string? Location);

public record TransferRequest(
    Guid FromAccountId,
    Guid ToAccountId,
    decimal Amount,
    string? Description);