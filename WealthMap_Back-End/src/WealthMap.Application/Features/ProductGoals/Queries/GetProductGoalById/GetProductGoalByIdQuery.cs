using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.ProductGoals.DTOs;

namespace WealthMap.Application.Features.ProductGoals.Queries.GetProductGoalById;

public record GetProductGoalByIdQuery(Guid Id, Guid UserId) : IQuery<ProductGoalDto>;
