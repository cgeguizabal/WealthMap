using FluentValidation;

namespace WealthMap.Application.Features.ProductGoals.Commands.CreateProductGoal;

public class CreateProductGoalValidator : AbstractValidator<CreateProductGoalCommand>
{
    public CreateProductGoalValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Goal name is required.")
            .MaximumLength(120);

        RuleFor(x => x.TargetAmount)
            .GreaterThan(0).WithMessage("Target amount must be greater than zero.");

        RuleFor(x => x.CurrentAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Current amount cannot be negative.")
            .When(x => x.CurrentAmount is not null);

        RuleFor(x => x.Currency)
            .NotEmpty()
            .Length(3).WithMessage("Currency must be a 3-letter ISO code.");

        RuleFor(x => x.Deadline)
            .Must(d => d!.Value >= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Deadline cannot be in the past.")
            .When(x => x.Deadline is not null);
    }
}
