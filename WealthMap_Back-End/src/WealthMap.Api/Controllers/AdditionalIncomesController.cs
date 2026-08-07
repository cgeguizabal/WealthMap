using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WealthMap.Api.Extensions;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.AdditionalIncomes.Commands.CreateAdditionalIncome;
using WealthMap.Application.Features.AdditionalIncomes.Commands.DeleteAdditionalIncome;
using WealthMap.Application.Features.AdditionalIncomes.Commands.UpdateAdditionalIncome;
using WealthMap.Application.Features.AdditionalIncomes.Queries.GetAdditionalIncomeById;
using WealthMap.Application.Features.AdditionalIncomes.Queries.GetAdditionalIncomes;

namespace WealthMap.Api.Controllers;

[ApiController]
[Route("api/v1/additional-incomes")]
[Authorize]
public class AdditionalIncomesController : ControllerBase
{
    private readonly ISender _sender;

    public AdditionalIncomesController(ISender sender) => _sender = sender;

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] AdditionalIncomeRequest request,
        CancellationToken ct)
    {
        var command = new CreateAdditionalIncomeCommand(
            User.GetUserId(),
            request.Name,
            request.Amount,
            request.Currency,
            request.Frequency,
            request.DepositAccountId);

        var result = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _sender.Send(new GetAdditionalIncomesQuery(User.GetUserId()), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new GetAdditionalIncomeByIdQuery(id, User.GetUserId()), ct);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] AdditionalIncomeRequest request,
        CancellationToken ct)
    {
        var command = new UpdateAdditionalIncomeCommand(
            id,
            User.GetUserId(),
            request.Name,
            request.Amount,
            request.Frequency,
            request.DepositAccountId);

        var result = await _sender.Send(command, ct);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _sender.Send(new DeleteAdditionalIncomeCommand(id, User.GetUserId()), ct);
        return NoContent();
    }
}

public record AdditionalIncomeRequest(
    string Name,
    decimal Amount,
    string Currency,
    int Frequency,
    Guid DepositAccountId);