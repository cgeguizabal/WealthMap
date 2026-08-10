using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WealthMap.Api.Extensions;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.CreditCards.Commands.CreateCreditCard;
using WealthMap.Application.Features.CreditCards.Commands.PayCreditCard;
using WealthMap.Application.Features.CreditCards.Commands.UpdateCreditCard;
using WealthMap.Application.Features.CreditCards.Commands.UpdateCreditCardLimit;
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
            request.StatementCutoffDay);

        var result = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _sender.Send(new GetCreditCardsQuery(User.GetUserId()), ct);
        return Ok(result);
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
    int StatementCutoffDay);

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