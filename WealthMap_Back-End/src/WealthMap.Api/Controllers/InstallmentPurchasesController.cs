using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WealthMap.Api.Extensions;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.InstallmentPurchases.Commands.CreateInstallmentPurchase;
using WealthMap.Application.Features.InstallmentPurchases.Commands.PayInstallment;
using WealthMap.Application.Features.InstallmentPurchases.Queries.GetInstallmentPurchaseById;
using WealthMap.Application.Features.InstallmentPurchases.Queries.GetInstallmentPurchases;

namespace WealthMap.Api.Controllers;

[ApiController]
[Route("api/v1/installment-purchases")]
[Authorize]
public class InstallmentPurchasesController : ControllerBase
{
    private readonly ISender _sender;

    public InstallmentPurchasesController(ISender sender) => _sender = sender;

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateInstallmentPurchaseRequest request,
        CancellationToken ct)
    {
        var command = new CreateInstallmentPurchaseCommand(
            User.GetUserId(),
            request.ProductName,
            request.TotalPrice,
            request.StoreId,
            request.CreditCardId,
            request.MonthsCount,
            request.PurchasedAt);

        var result = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _sender.Send(new GetInstallmentPurchasesQuery(User.GetUserId()), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new GetInstallmentPurchaseByIdQuery(id, User.GetUserId()), ct);
        return Ok(result);
    }

    [HttpPost("{id:guid}/pay")]
    public async Task<IActionResult> Pay(
        Guid id,
        [FromBody] PayInstallmentRequest request,
        CancellationToken ct)
    {
        var command = new PayInstallmentCommand(
            id,
            User.GetUserId(),
            request.SourceType,
            request.SourceAccountId);

        var result = await _sender.Send(command, ct);
        return Ok(result);
    }
}

public record CreateInstallmentPurchaseRequest(
    string ProductName,
    decimal TotalPrice,
    Guid? StoreId,
    Guid CreditCardId,
    int MonthsCount,
    DateOnly? PurchasedAt);

public record PayInstallmentRequest(
    string SourceType,
    Guid? SourceAccountId);
