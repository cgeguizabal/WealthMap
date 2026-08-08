using FluentValidation;

namespace WealthMap.Application.Features.SavingsGoals.Commands.ContributeToSavingsGoal;

public class ContributeToSavingsGoalValidator : AbstractValidator<ContributeToSavingsGoalCommand>
{
    public ContributeToSavingsGoalValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Contribution must be greater than zero.");
    }
}
