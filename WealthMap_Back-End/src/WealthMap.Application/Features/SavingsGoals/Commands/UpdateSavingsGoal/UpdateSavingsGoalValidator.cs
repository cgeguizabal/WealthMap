using FluentValidation;

namespace WealthMap.Application.Features.SavingsGoals.Commands.UpdateSavingsGoal;

public class UpdateSavingsGoalValidator : AbstractValidator<UpdateSavingsGoalCommand>
{
    public UpdateSavingsGoalValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Goal name is required.")
            .MaximumLength(120);

        RuleFor(x => x.TargetAmount)
            .GreaterThan(0).WithMessage("Target amount must be greater than zero.");
    }
}
