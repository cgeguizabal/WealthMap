using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.SavingsGoals.DTOs;

namespace WealthMap.Application.Features.SavingsGoals.Queries.GetSavingsGoalById;

public record GetSavingsGoalByIdQuery(Guid Id, Guid UserId) : IQuery<SavingsGoalDto>;
