using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.ProductGoals.DTOs;

namespace WealthMap.Application.Features.ProductGoals.Queries.GetProductGoals;

public class GetProductGoalsHandler : IQueryHandler<GetProductGoalsQuery, IReadOnlyList<ProductGoalDto>>
{
    private readonly IProductGoalRepository _goals;

    public GetProductGoalsHandler(IProductGoalRepository goals) => _goals = goals;

    public async Task<IReadOnlyList<ProductGoalDto>> Handle(GetProductGoalsQuery request, CancellationToken ct)
    {
        var goals = await _goals.GetAllForUserAsync(request.UserId, ct);
        return goals.Select(ProductGoalDto.FromEntity).ToList();
    }
}
