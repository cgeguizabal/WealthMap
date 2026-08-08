using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.ProductGoals.DTOs;

namespace WealthMap.Application.Features.ProductGoals.Queries.GetProductGoals;

public record GetProductGoalsQuery(Guid UserId) : IQuery<IReadOnlyList<ProductGoalDto>>;
