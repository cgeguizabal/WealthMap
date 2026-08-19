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
using WealthMap.Application.Features.CardIncidents.Commands.MarkCardRecovered;
using WealthMap.Application.Features.CardIncidents.Commands.ReplaceCard;
using WealthMap.Application.Features.CardIncidents.Commands.ReportCardLost;
using WealthMap.Application.Features.CardIncidents.Queries.GetCardIncidents;
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

    /// <summary>
    /// Records that the card was lost, stolen, damaged or compromised.
    /// </summary>
    /// <remarks>
    /// The card stays exactly as it is apart from going out of service: the balance
    /// is still owed, the statement still falls due, and every purchase on it keeps
    /// its history. What changes is that its remaining credit stops counting toward
    /// safe-to-spend, because the user has no way to reach it.
    /// </remarks>
    [HttpPost("{id:guid}/loss-report")]
    public async Task<IActionResult> ReportLost(
        Guid id,
        [FromBody] ReportCardLostRequest request,
        CancellationToken ct)
    {
        var command = new ReportCardLostCommand(
            User.GetUserId(),
            CardKind.CreditCard,
            id,
            (CardLossReason)request.Reason,
            request.ReportedOn,
            request.Notes);

        return Ok(await _sender.Send(command, ct));
    }

    /// <summary>
    /// Records the replacement card the bank sent, and the number it carries.
    /// </summary>
    [HttpPost("{id:guid}/replacement")]
    public async Task<IActionResult> Replace(
        Guid id,
        [FromBody] ReplaceCardRequest request,
        CancellationToken ct)
    {
        var command = new ReplaceCardCommand(
            User.GetUserId(),
            CardKind.CreditCard,
            id,
            request.NewLastFour,
            request.ReplacedOn,
            request.Notes);

        return Ok(await _sender.Send(command, ct));
    }

    /// <summary>Closes the report because the card turned up.</summary>
    [HttpPost("{id:guid}/recovery")]
    public async Task<IActionResult> MarkRecovered(
        Guid id,
        [FromBody] MarkCardRecoveredRequest request,
        CancellationToken ct)
    {
        var command = new MarkCardRecoveredCommand(
            User.GetUserId(), CardKind.CreditCard, id, request.RecoveredOn, request.Notes);

        return Ok(await _sender.Send(command, ct));
    }

    /// <summary>Every time this card was reported, and how each report ended.</summary>
    [HttpGet("{id:guid}/incidents")]
    public async Task<IActionResult> GetIncidents(Guid id, CancellationToken ct)
    {
        var query = new GetCardIncidentsQuery(User.GetUserId(), CardKind.CreditCard, id);

        return Ok(await _sender.Send(query, ct));
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

/// <param name="Reason">1 Lost, 2 Stolen, 3 Damaged, 4 Compromised.</param>
/// <param name="ReportedOn">The user's own date. Omitted means today.</param>
public record ReportCardLostRequest(int Reason, DateOnly? ReportedOn, string? Notes);

/// <param name="NewLastFour">
/// The replacement's digits. Omitted or null means the bank reissued the same
/// number, and the recorded one is left alone.
/// </param>
public record ReplaceCardRequest(string? NewLastFour, DateOnly? ReplacedOn, string? Notes);

public record MarkCardRecoveredRequest(DateOnly? RecoveredOn, string? Notes);
