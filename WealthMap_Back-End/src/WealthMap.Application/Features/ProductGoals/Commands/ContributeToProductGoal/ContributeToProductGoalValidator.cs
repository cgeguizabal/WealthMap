using FluentValidation;

namespace WealthMap.Application.Features.ProductGoals.Commands.ContributeToProductGoal;

public class ContributeToProductGoalValidator : AbstractValidator<ContributeToProductGoalCommand>
{
    public ContributeToProductGoalValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Contribution must be greater than zero.");
    }
}
