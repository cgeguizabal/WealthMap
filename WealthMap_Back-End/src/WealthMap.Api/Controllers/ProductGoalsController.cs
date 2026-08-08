using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WealthMap.Api.Extensions;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.ProductGoals.Commands.ContributeToProductGoal;
using WealthMap.Application.Features.ProductGoals.Commands.CreateProductGoal;
using WealthMap.Application.Features.ProductGoals.Commands.DeleteProductGoal;
using WealthMap.Application.Features.ProductGoals.Commands.UpdateProductGoal;
using WealthMap.Application.Features.ProductGoals.Queries.GetProductGoalById;
using WealthMap.Application.Features.ProductGoals.Queries.GetProductGoals;

namespace WealthMap.Api.Controllers;

[ApiController]
[Route("api/v1/product-goals")]
[Authorize]
public class ProductGoalsController : ControllerBase
{
    private readonly ISender _sender;

    public ProductGoalsController(ISender sender) => _sender = sender;

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateProductGoalRequest request,
        CancellationToken ct)
    {
        var command = new CreateProductGoalCommand(
            User.GetUserId(),
            request.Name,
            request.TargetAmount,
            request.Currency,
            request.CurrentAmount,
            request.Deadline);

        var result = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _sender.Send(new GetProductGoalsQuery(User.GetUserId()), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new GetProductGoalByIdQuery(id, User.GetUserId()), ct);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateProductGoalRequest request,
        CancellationToken ct)
    {
        var command = new UpdateProductGoalCommand(
            id,
            User.GetUserId(),
            request.Name,
            request.TargetAmount,
            request.Deadline);

        var result = await _sender.Send(command, ct);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _sender.Send(new DeleteProductGoalCommand(id, User.GetUserId()), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/contribute")]
    public async Task<IActionResult> Contribute(
        Guid id,
        [FromBody] ProductGoalContributeRequest request,
        CancellationToken ct)
    {
        var command = new ContributeToProductGoalCommand(id, User.GetUserId(), request.Amount);
        var result = await _sender.Send(command, ct);
        return Ok(result);
    }
}

public record CreateProductGoalRequest(
    string Name,
    decimal TargetAmount,
    string Currency,
    decimal? CurrentAmount,
    DateOnly? Deadline);

public record UpdateProductGoalRequest(
    string Name,
    decimal TargetAmount,
    DateOnly? Deadline);

public record ProductGoalContributeRequest(decimal Amount);
