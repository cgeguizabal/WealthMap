using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WealthMap.Api.Extensions;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.SavingsGoals.Commands.ContributeToSavingsGoal;
using WealthMap.Application.Features.SavingsGoals.Commands.CreateSavingsGoal;
using WealthMap.Application.Features.SavingsGoals.Commands.DeleteSavingsGoal;
using WealthMap.Application.Features.SavingsGoals.Commands.UpdateSavingsGoal;
using WealthMap.Application.Features.SavingsGoals.Queries.GetSavingsGoalById;
using WealthMap.Application.Features.SavingsGoals.Queries.GetSavingsGoals;

namespace WealthMap.Api.Controllers;

[ApiController]
[Route("api/v1/savings-goals")]
[Authorize]
public class SavingsGoalsController : ControllerBase
{
    private readonly ISender _sender;

    public SavingsGoalsController(ISender sender) => _sender = sender;

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateSavingsGoalRequest request,
        CancellationToken ct)
    {
        var command = new CreateSavingsGoalCommand(
            User.GetUserId(),
            request.Name,
            request.TargetAmount,
            request.Currency,
            request.CurrentAmount,
            request.Deadline,
            request.LinkedAccountId);

        var result = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _sender.Send(new GetSavingsGoalsQuery(User.GetUserId()), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new GetSavingsGoalByIdQuery(id, User.GetUserId()), ct);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateSavingsGoalRequest request,
        CancellationToken ct)
    {
        var command = new UpdateSavingsGoalCommand(
            id,
            User.GetUserId(),
            request.Name,
            request.TargetAmount,
            request.Deadline,
            request.LinkedAccountId);

        var result = await _sender.Send(command, ct);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _sender.Send(new DeleteSavingsGoalCommand(id, User.GetUserId()), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/contribute")]
    public async Task<IActionResult> Contribute(
        Guid id,
        [FromBody] ContributeRequest request,
        CancellationToken ct)
    {
        var command = new ContributeToSavingsGoalCommand(
            id,
            User.GetUserId(),
            request.Amount,
            request.SourceAccountId);

        var result = await _sender.Send(command, ct);
        return Ok(result);
    }
}

public record CreateSavingsGoalRequest(
    string Name,
    decimal TargetAmount,
    string Currency,
    decimal? CurrentAmount,
    DateOnly Deadline,
    Guid? LinkedAccountId);

public record UpdateSavingsGoalRequest(
    string Name,
    decimal TargetAmount,
    DateOnly Deadline,
    Guid? LinkedAccountId);

public record ContributeRequest(
    decimal Amount,
    Guid? SourceAccountId);
