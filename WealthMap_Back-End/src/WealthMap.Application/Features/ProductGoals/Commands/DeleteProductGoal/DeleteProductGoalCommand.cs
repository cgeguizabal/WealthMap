using WealthMap.Application.Common.Messaging;

namespace WealthMap.Application.Features.ProductGoals.Commands.DeleteProductGoal;

public record DeleteProductGoalCommand(Guid Id, Guid UserId) : ICommand<bool>;
