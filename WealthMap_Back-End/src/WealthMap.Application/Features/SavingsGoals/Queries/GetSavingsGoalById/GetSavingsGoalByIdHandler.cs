using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.SavingsGoals.DTOs;

namespace WealthMap.Application.Features.SavingsGoals.Queries.GetSavingsGoalById;

public class GetSavingsGoalByIdHandler : IQueryHandler<GetSavingsGoalByIdQuery, SavingsGoalDto>
{
    private readonly ISavingsGoalRepository _goals;

    public GetSavingsGoalByIdHandler(ISavingsGoalRepository goals) => _goals = goals;

    public async Task<SavingsGoalDto> Handle(GetSavingsGoalByIdQuery request, CancellationToken ct)
    {
        var goal = await _goals.GetByIdForUserAsync(request.Id, request.UserId, ct)
            ?? throw new NotFoundException("SavingsGoal", request.Id);

        return SavingsGoalDto.FromEntity(goal);
    }
}
