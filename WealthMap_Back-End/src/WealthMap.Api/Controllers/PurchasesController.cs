using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WealthMap.Api.Extensions;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Purchases.Commands.CreatePurchase;
using WealthMap.Application.Features.Purchases.Commands.UpdatePurchase;
using WealthMap.Application.Features.Purchases.Commands.DeletePurchase;
using WealthMap.Application.Features.Purchases.Queries.GetPurchaseById;
using WealthMap.Application.Features.Purchases.Queries.GetPurchases;

namespace WealthMap.Api.Controllers;

[ApiController]
[Route("api/v1/purchases")]
[Authorize]
public class PurchasesController : ControllerBase
{
    private readonly ISender _sender;

    public PurchasesController(ISender sender) => _sender = sender;

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreatePurchaseRequest request,
        CancellationToken ct)
    {
        var command = new CreatePurchaseCommand(
            User.GetUserId(),
            request.ProductName,
            request.Amount,
            request.Currency,
            request.OccurredAt,
            request.StoreId,
            request.Category,
            request.PaymentMethod,
            request.AccountId,
            request.CreditCardId,
            request.Notes);

        var result = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken ct,
        [FromQuery] int? year = null,
        [FromQuery] int? month = null,
        [FromQuery] string? category = null,
        [FromQuery] Guid? creditCardId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetPurchasesQuery(
            User.GetUserId(), year, month, category, creditCardId, page, pageSize);
        var result = await _sender.Send(query, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new GetPurchaseByIdQuery(id, User.GetUserId()), ct);
        return Ok(result);
    }

    /// <summary>
    /// Corrects a purchase. The money it moved is reversed and reapplied, so the
    /// method and instrument can change too.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] CreatePurchaseRequest request,
        CancellationToken ct)
    {
        var command = new UpdatePurchaseCommand(
            id,
            User.GetUserId(),
            request.ProductName,
            request.Amount,
            request.Currency,
            request.OccurredAt,
            request.StoreId,
            request.Category,
            request.PaymentMethod,
            request.AccountId,
            request.CreditCardId,
            request.Notes);

        return Ok(await _sender.Send(command, ct));
    }

    /// <summary>
    /// Removes a purchase recorded in error, restoring the account or card it
    /// touched. A real delete — unlike accounts and cards, which archive.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _sender.Send(new DeletePurchaseCommand(id, User.GetUserId()), ct);
        return NoContent();
    }
}

public record CreatePurchaseRequest(
    string ProductName,
    decimal Amount,
    string? Currency,
    DateTime? OccurredAt,
    Guid? StoreId,
    string Category,
    int PaymentMethod,
    Guid? AccountId,
    Guid? CreditCardId,
    string? Notes);
