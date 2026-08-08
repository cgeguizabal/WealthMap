using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.SavingsGoals.DTOs;

namespace WealthMap.Application.Features.SavingsGoals.Queries.GetSavingsGoals;

public record GetSavingsGoalsQuery(Guid UserId) : IQuery<IReadOnlyList<SavingsGoalDto>>;
