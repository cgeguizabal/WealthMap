using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WealthMap.Api.Extensions;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Jobs.Commands.AddDeduction;
using WealthMap.Application.Features.Jobs.Commands.CreateJob;
using WealthMap.Application.Features.Jobs.Commands.DeleteJob;
using WealthMap.Application.Features.Jobs.Commands.RemoveDeduction;
using WealthMap.Application.Features.Jobs.Commands.UpdateDeduction;
using WealthMap.Application.Features.Jobs.Commands.UpdateJob;
using WealthMap.Application.Features.Jobs.Queries.GetJobById;
using WealthMap.Application.Features.Jobs.Queries.GetJobs;

namespace WealthMap.Api.Controllers;

[ApiController]
[Route("api/v1/jobs")]
[Authorize]
public class JobsController : ControllerBase
{
    private readonly ISender _sender;

    public JobsController(ISender sender) => _sender = sender;

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateJobRequest request,
        CancellationToken ct)
    {
        var command = new CreateJobCommand(
            User.GetUserId(),
            request.Title,
            request.Employer,
            request.GrossMonthlySalary,
            request.Currency,
            request.DepositAccountId,
            request.PaymentDays);

        var result = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _sender.Send(new GetJobsQuery(User.GetUserId()), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new GetJobByIdQuery(id, User.GetUserId()), ct);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateJobRequest request,
        CancellationToken ct)
    {
        var command = new UpdateJobCommand(
            id,
            User.GetUserId(),
            request.Title,
            request.Employer,
            request.GrossMonthlySalary,
            request.DepositAccountId,
            request.PaymentDays);

        var result = await _sender.Send(command, ct);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _sender.Send(new DeleteJobCommand(id, User.GetUserId()), ct);
        return NoContent();
    }

    [HttpPost("{jobId:guid}/deductions")]
    public async Task<IActionResult> AddDeduction(
        Guid jobId,
        [FromBody] DeductionRequest request,
        CancellationToken ct)
    {
        var command = new AddDeductionCommand(
            jobId,
            User.GetUserId(),
            request.Name,
            request.Type,
            request.Value);

        var result = await _sender.Send(command, ct);
        return Ok(result);
    }

    [HttpPut("{jobId:guid}/deductions/{deductionId:guid}")]
    public async Task<IActionResult> UpdateDeduction(
        Guid jobId,
        Guid deductionId,
        [FromBody] DeductionRequest request,
        CancellationToken ct)
    {
        var command = new UpdateDeductionCommand(
            jobId,
            deductionId,
            User.GetUserId(),
            request.Name,
            request.Type,
            request.Value);

        var result = await _sender.Send(command, ct);
        return Ok(result);
    }

    [HttpDelete("{jobId:guid}/deductions/{deductionId:guid}")]
    public async Task<IActionResult> RemoveDeduction(
        Guid jobId,
        Guid deductionId,
        CancellationToken ct)
    {
        var command = new RemoveDeductionCommand(jobId, deductionId, User.GetUserId());
        var result = await _sender.Send(command, ct);
        return Ok(result);
    }
}

public record CreateJobRequest(
    string Title,
    string Employer,
    decimal GrossMonthlySalary,
    string Currency,
    Guid DepositAccountId,
    IReadOnlyList<int> PaymentDays);

public record UpdateJobRequest(
    string Title,
    string Employer,
    decimal GrossMonthlySalary,
    Guid DepositAccountId,
    IReadOnlyList<int> PaymentDays);

public record DeductionRequest(
    string Name,
    int Type,
    decimal Value);