using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.SavingsGoals.DTOs;

namespace WealthMap.Application.Features.SavingsGoals.Queries.GetSavingsGoals;

public class GetSavingsGoalsHandler : IQueryHandler<GetSavingsGoalsQuery, IReadOnlyList<SavingsGoalDto>>
{
    private readonly ISavingsGoalRepository _goals;

    public GetSavingsGoalsHandler(ISavingsGoalRepository goals) => _goals = goals;

    public async Task<IReadOnlyList<SavingsGoalDto>> Handle(GetSavingsGoalsQuery request, CancellationToken ct)
    {
        var goals = await _goals.GetAllForUserAsync(request.UserId, ct);
        return goals.Select(SavingsGoalDto.FromEntity).ToList();
    }
}
