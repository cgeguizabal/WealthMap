using WealthMap.Application.Common.Messaging;

namespace WealthMap.Application.Features.SavingsGoals.Commands.DeleteSavingsGoal;

public record DeleteSavingsGoalCommand(Guid Id, Guid UserId) : ICommand<bool>;
