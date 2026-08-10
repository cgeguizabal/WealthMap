using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WealthMap.Api.Extensions;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Debts.Commands.CreateDebt;
using WealthMap.Application.Features.Debts.Commands.DeleteDebt;
using WealthMap.Application.Features.Debts.Commands.MarkDebtDefaulted;
using WealthMap.Application.Features.Debts.Commands.PayDebt;
using WealthMap.Application.Features.Debts.Commands.UpdateDebt;
using WealthMap.Application.Features.Debts.Queries.GetDebtById;
using WealthMap.Application.Features.Debts.Queries.GetDebts;
using WealthMap.Application.Features.Payments.Queries.GetPaymentsForTarget;
using WealthMap.Domain.Enums;

namespace WealthMap.Api.Controllers;

[ApiController]
[Route("api/v1/debts")]
[Authorize]
public class DebtsController : ControllerBase
{
    private readonly ISender _sender;

    public DebtsController(ISender sender) => _sender = sender;

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateDebtRequest request,
        CancellationToken ct)
    {
        var command = new CreateDebtCommand(
            User.GetUserId(),
            request.Name,
            request.OriginalAmount,
            request.RemainingAmount,
            request.Currency,
            request.MonthlyPayment,
            request.MonthlyDueDay);

        var result = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _sender.Send(new GetDebtsQuery(User.GetUserId()), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new GetDebtByIdQuery(id, User.GetUserId()), ct);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateDebtRequest request,
        CancellationToken ct)
    {
        var command = new UpdateDebtCommand(
            id,
            User.GetUserId(),
            request.Name,
            request.MonthlyPayment,
            request.MonthlyDueDay);

        var result = await _sender.Send(command, ct);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _sender.Send(new DeleteDebtCommand(id, User.GetUserId()), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/payments")]
    public async Task<IActionResult> Pay(
        Guid id,
        [FromBody] PayDebtRequest request,
        CancellationToken ct)
    {
        var command = new PayDebtCommand(
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
        var query = new GetPaymentsForTargetQuery(User.GetUserId(), PaymentTargetType.Debt, id);
        var result = await _sender.Send(query, ct);
        return Ok(result);
    }

    [HttpPost("{id:guid}/default")]
    public async Task<IActionResult> MarkDefaulted(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new MarkDebtDefaultedCommand(id, User.GetUserId()), ct);
        return Ok(result);
    }
}

public record CreateDebtRequest(
    string Name,
    decimal OriginalAmount,
    decimal? RemainingAmount,
    string Currency,
    decimal MonthlyPayment,
    int MonthlyDueDay);

public record UpdateDebtRequest(
    string Name,
    decimal MonthlyPayment,
    int MonthlyDueDay);

public record PayDebtRequest(
    decimal Amount,
    string SourceType,
    Guid? SourceAccountId,
    string? Notes);
