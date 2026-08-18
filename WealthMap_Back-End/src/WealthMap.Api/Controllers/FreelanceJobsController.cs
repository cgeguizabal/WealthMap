using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WealthMap.Api.Extensions;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.FreelanceJobs.Commands.CancelFreelanceJob;
using WealthMap.Application.Features.FreelanceJobs.Commands.CreateFreelanceJob;
using WealthMap.Application.Features.FreelanceJobs.Commands.DeleteFreelanceJob;
using WealthMap.Application.Features.FreelanceJobs.Commands.MarkFreelanceJobDelivered;
using WealthMap.Application.Features.FreelanceJobs.Commands.MarkFreelanceJobPaid;
using WealthMap.Application.Features.FreelanceJobs.Commands.ReopenFreelanceJob;
using WealthMap.Application.Features.FreelanceJobs.Commands.UpdateFreelanceJob;
using WealthMap.Application.Features.FreelanceJobs.Queries.GetFreelanceJobById;
using WealthMap.Application.Features.FreelanceJobs.Queries.GetFreelanceJobs;

namespace WealthMap.Api.Controllers;

/// <summary>
/// Freelance work: agreed, delivered and paid on no schedule at all.
/// </summary>
/// <remarks>
/// Every state change is a separate endpoint rather than a PUT that accepts a
/// status. Delivering and being paid are different events, happen at different
/// times, and only one of them moves money — collapsing them into one update
/// would hide that.
/// </remarks>
[ApiController]
[Route("api/v1/freelance-jobs")]
[Authorize]
public class FreelanceJobsController : ControllerBase
{
    private readonly ISender _sender;

    public FreelanceJobsController(ISender sender) => _sender = sender;

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateFreelanceJobRequest request,
        CancellationToken ct)
    {
        var command = new CreateFreelanceJobCommand(
            User.GetUserId(),
            request.Title,
            request.AgreedAmount,
            request.Currency,
            request.Client,
            request.DueOn,
            request.Notes);

        var result = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _sender.Send(new GetFreelanceJobsQuery(User.GetUserId()), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new GetFreelanceJobByIdQuery(id, User.GetUserId()), ct);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateFreelanceJobRequest request,
        CancellationToken ct)
    {
        var command = new UpdateFreelanceJobCommand(
            id,
            User.GetUserId(),
            request.Title,
            request.AgreedAmount,
            request.Currency,
            request.Client,
            request.DueOn,
            request.Notes);

        return Ok(await _sender.Send(command, ct));
    }

    /// <summary>Records that the work was finished. Moves no money.</summary>
    [HttpPost("{id:guid}/delivered")]
    public async Task<IActionResult> MarkDelivered(
        Guid id,
        [FromBody] MarkDeliveredRequest request,
        CancellationToken ct)
    {
        var command = new MarkFreelanceJobDeliveredCommand(id, User.GetUserId(), request.DeliveredOn);

        return Ok(await _sender.Send(command, ct));
    }

    /// <summary>Records the payment, deposits it, and writes the movement.</summary>
    [HttpPost("{id:guid}/paid")]
    public async Task<IActionResult> MarkPaid(
        Guid id,
        [FromBody] MarkPaidRequest request,
        CancellationToken ct)
    {
        var command = new MarkFreelanceJobPaidCommand(
            id,
            User.GetUserId(),
            request.AmountPaid,
            request.DepositAccountId,
            request.PaidOn);

        return Ok(await _sender.Send(command, ct));
    }

    /// <summary>The work was called off. Keeps the row; delete removes it.</summary>
    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(
        Guid id,
        [FromBody] CancelRequest request,
        CancellationToken ct)
    {
        var command = new CancelFreelanceJobCommand(id, User.GetUserId(), request.CancelledOn);

        return Ok(await _sender.Send(command, ct));
    }

    /// <summary>The client came back. Only cancelled work can be reopened.</summary>
    [HttpPost("{id:guid}/reopen")]
    public async Task<IActionResult> Reopen(Guid id, CancellationToken ct)
        => Ok(await _sender.Send(new ReopenFreelanceJobCommand(id, User.GetUserId()), ct));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _sender.Send(new DeleteFreelanceJobCommand(id, User.GetUserId()), ct);
        return NoContent();
    }

    public record CreateFreelanceJobRequest(
        string Title,
        decimal AgreedAmount,
        string Currency,
        string? Client,
        DateOnly? DueOn,
        string? Notes);

    public record UpdateFreelanceJobRequest(
        string Title,
        decimal AgreedAmount,
        string Currency,
        string? Client,
        DateOnly? DueOn,
        string? Notes);

    public record MarkDeliveredRequest(DateOnly DeliveredOn);

    public record MarkPaidRequest(decimal AmountPaid, Guid DepositAccountId, DateOnly PaidOn);

    public record CancelRequest(DateOnly CancelledOn);
}
