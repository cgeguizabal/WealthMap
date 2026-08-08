using FluentValidation;

namespace WealthMap.Application.Features.ProductGoals.Commands.UpdateProductGoal;

public class UpdateProductGoalValidator : AbstractValidator<UpdateProductGoalCommand>
{
    public UpdateProductGoalValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Goal name is required.")
            .MaximumLength(120);

        RuleFor(x => x.TargetAmount)
            .GreaterThan(0).WithMessage("Target amount must be greater than zero.");
    }
}
