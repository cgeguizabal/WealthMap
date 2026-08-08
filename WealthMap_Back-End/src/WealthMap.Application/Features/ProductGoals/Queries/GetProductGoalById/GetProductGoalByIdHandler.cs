using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.ProductGoals.DTOs;

namespace WealthMap.Application.Features.ProductGoals.Queries.GetProductGoalById;

public class GetProductGoalByIdHandler : IQueryHandler<GetProductGoalByIdQuery, ProductGoalDto>
{
    private readonly IProductGoalRepository _goals;

    public GetProductGoalByIdHandler(IProductGoalRepository goals) => _goals = goals;

    public async Task<ProductGoalDto> Handle(GetProductGoalByIdQuery request, CancellationToken ct)
    {
        var goal = await _goals.GetByIdForUserAsync(request.Id, request.UserId, ct)
            ?? throw new NotFoundException("ProductGoal", request.Id);

        return ProductGoalDto.FromEntity(goal);
    }
}
