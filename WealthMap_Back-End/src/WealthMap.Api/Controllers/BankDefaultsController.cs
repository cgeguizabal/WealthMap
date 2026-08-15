using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WealthMap.Api.Extensions;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.BankDefaults.Commands.DeleteBankDefault;
using WealthMap.Application.Features.BankDefaults.Commands.UpsertBankDefault;
using WealthMap.Application.Features.BankDefaults.Queries.GetBankDefaults;

namespace WealthMap.Api.Controllers;

/// <summary>
/// Which account to assume when a bank's transfer notification does not name one.
/// </summary>
/// <remarks>
/// Stored now, consumed by nothing. See "Planned: automatic transaction sync" in
/// docs/PROJECT_GUIDE.md for why the data exists ahead of the feature.
/// </remarks>
[ApiController]
[Authorize]
[Route("api/v1/bank-defaults")]
public class BankDefaultsController : ControllerBase
{
    private readonly ISender _sender;

    public BankDefaultsController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await _sender.Send(new GetBankDefaultsQuery(User.GetUserId()), ct));

    /// <summary>
    /// Upsert on (bank, direction). PUT rather than POST because the request is
    /// idempotent: sending it twice leaves exactly one default, not two.
    /// </summary>
    [HttpPut]
    public async Task<IActionResult> Upsert(
        [FromBody] UpsertBankDefaultRequest request,
        CancellationToken ct)
    {
        var command = new UpsertBankDefaultCommand(
            User.GetUserId(), request.BankName, request.Direction, request.DefaultAccountId);

        return Ok(await _sender.Send(command, ct));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _sender.Send(new DeleteBankDefaultCommand(id, User.GetUserId()), ct);
        return NoContent();
    }
}

public record UpsertBankDefaultRequest(
    string BankName,
    int Direction,
    Guid DefaultAccountId);
