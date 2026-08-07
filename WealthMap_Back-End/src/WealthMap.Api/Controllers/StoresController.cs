using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WealthMap.Api.Extensions;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Stores.Commands.CreateStore;
using WealthMap.Application.Features.Stores.Commands.UpdateStore;
using WealthMap.Application.Features.Stores.Queries.GetStoreById;
using WealthMap.Application.Features.Stores.Queries.GetStores;

namespace WealthMap.Api.Controllers;

[ApiController]
[Route("api/v1/stores")]
[Authorize]
public class StoresController : ControllerBase
{
    private readonly ISender _sender;

    public StoresController(ISender sender) => _sender = sender;

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] StoreRequest request,
        CancellationToken ct)
    {
        var command = new CreateStoreCommand(
            User.GetUserId(),
            request.Name,
            request.Category,
            request.LogoUrl,
            request.Description);

        var result = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _sender.Send(new GetStoresQuery(User.GetUserId()), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new GetStoreByIdQuery(id, User.GetUserId()), ct);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] StoreRequest request,
        CancellationToken ct)
    {
        var command = new UpdateStoreCommand(
            id,
            User.GetUserId(),
            request.Name,
            request.Category,
            request.LogoUrl,
            request.Description);

        var result = await _sender.Send(command, ct);
        return Ok(result);
    }
}

public record StoreRequest(
    string Name,
    string Category,
    string? LogoUrl,
    string? Description);