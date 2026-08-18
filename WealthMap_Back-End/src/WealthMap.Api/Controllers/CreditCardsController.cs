using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WealthMap.Api.Extensions;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.CreditCards.Commands.CreateCreditCard;
using WealthMap.Application.Features.CreditCards.Commands.UpdateCreditCardTracking;
using WealthMap.Application.Features.CreditCards.Commands.PayCreditCard;
using WealthMap.Application.Features.CreditCards.Commands.RestoreCreditCard;
using WealthMap.Application.Features.CreditCards.Commands.UpdateCreditCard;
using WealthMap.Application.Features.CreditCards.Commands.UpdateCreditCardLimit;
using WealthMap.Application.Features.CreditCards.Commands.ArchiveCreditCard;
using WealthMap.Application.Features.CreditCards.Queries.GetCreditCardById;
using WealthMap.Application.Features.CreditCards.Queries.GetCreditCards;
using WealthMap.Application.Features.Payments.Queries.GetPaymentsForTarget;
using WealthMap.Domain.Enums;

namespace WealthMap.Api.Controllers;

[ApiController]
[Route("api/v1/credit-cards")]
[Authorize]
public class CreditCardsController : ControllerBase
{
    private readonly ISender _sender;

    public CreditCardsController(ISender sender) => _sender = sender;

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateCreditCardRequest request,
        CancellationToken ct)
    {
        var command = new CreateCreditCardCommand(
            User.GetUserId(),
            request.CardName,
            request.BankName,
            request.CreditLimit,
            request.Currency,
            request.AnnualInterestRate,
            request.PaymentDueDay,
            request.StatementCutoffDay,
            request.LastFour,
            request.TrackingMode);

        var result = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <param name="includeArchived">
    /// Archived cards are hidden by default. Ask for them to offer restoring one.
    /// </param>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken ct,
        [FromQuery] bool includeArchived = false)
    {
        var query = new GetCreditCardsQuery(User.GetUserId(), includeArchived);

        return Ok(await _sender.Send(query, ct));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new GetCreditCardByIdQuery(id, User.GetUserId()), ct);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateCreditCardRequest request,
        CancellationToken ct)
    {
        var command = new UpdateCreditCardCommand(
            id,
            User.GetUserId(),
            request.CardName,
            request.BankName,
            request.AnnualInterestRate,
            request.PaymentDueDay,
            request.StatementCutoffDay,
            request.Notes);

        var result = await _sender.Send(command, ct);
        return Ok(result);
    }

    /// <summary>
    /// Archives the card: it leaves the user's lists and totals, but the
    /// purchases, installment plans and payments that reference it are preserved.
    /// </summary>
    /// <summary>Brings an archived card back into the lists and totals.</summary>
    [HttpPost("{id:guid}/restore")]
    public async Task<IActionResult> Restore(Guid id, CancellationToken ct)
    {
        await _sender.Send(new RestoreCreditCardCommand(id, User.GetUserId()), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _sender.Send(new ArchiveCreditCardCommand(id, User.GetUserId()), ct);
        return NoContent();
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
        var command = new UpdateCreditCardTrackingCommand(
            id, User.GetUserId(), request.TrackingMode, request.LastFour);

        return Ok(await _sender.Send(command, ct));
    }

    [HttpPut("{id:guid}/limit")]
    public async Task<IActionResult> UpdateLimit(
        Guid id,
        [FromBody] UpdateCreditCardLimitRequest request,
        CancellationToken ct)
    {
        var command = new UpdateCreditCardLimitCommand(id, User.GetUserId(), request.NewLimit);
        var result = await _sender.Send(command, ct);
        return Ok(result);
    }

    [HttpPost("{id:guid}/payments")]
    public async Task<IActionResult> Pay(
        Guid id,
        [FromBody] PayCreditCardRequest request,
        CancellationToken ct)
    {
        var command = new PayCreditCardCommand(
            id,
            User.GetUserId(),
            request.Amount,
            request.SourceType,
            request.SourceAccountId,
            request.Notes);

        var result = await _sender.Send(command, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}/payments")]
    public async Task<IActionResult> GetPayments(Guid id, CancellationToken ct)
    {
        var query = new GetPaymentsForTargetQuery(User.GetUserId(), PaymentTargetType.CreditCard, id);
        var result = await _sender.Send(query, ct);
        return Ok(result);
    }
}

public record CreateCreditCardRequest(
    string CardName,
    string BankName,
    decimal CreditLimit,
    string Currency,
    decimal AnnualInterestRate,
    int PaymentDueDay,
    int StatementCutoffDay,
    string? LastFour = null,
    int? TrackingMode = null);

public record UpdateCreditCardRequest(
    string CardName,
    string BankName,
    decimal AnnualInterestRate,
    int PaymentDueDay,
    int StatementCutoffDay,
    string? Notes);

public record UpdateCreditCardLimitRequest(decimal NewLimit);

public record PayCreditCardRequest(
    decimal Amount,
    string SourceType,
    Guid? SourceAccountId,
    string? Notes);