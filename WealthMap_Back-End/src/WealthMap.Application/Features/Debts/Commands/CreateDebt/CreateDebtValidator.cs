using FluentValidation;

namespace WealthMap.Application.Features.Debts.Commands.CreateDebt;

public class CreateDebtValidator : AbstractValidator<CreateDebtCommand>
{
    public CreateDebtValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Debt name is required.")
            .MaximumLength(120);

        RuleFor(x => x.OriginalAmount)
            .GreaterThan(0).WithMessage("Original amount must be greater than zero.");

        RuleFor(x => x.RemainingAmount)
            .GreaterThan(0).WithMessage("Remaining amount must be greater than zero.")
            .LessThanOrEqualTo(x => x.OriginalAmount)
            .WithMessage("Remaining amount cannot exceed the original amount.")
            .When(x => x.RemainingAmount is not null);

        RuleFor(x => x.Currency)
            .NotEmpty()
            .Length(3).WithMessage("Currency must be a 3-letter ISO code.");

        RuleFor(x => x.MonthlyPayment)
            .GreaterThan(0).WithMessage("Monthly payment must be greater than zero.");

        RuleFor(x => x.MonthlyDueDay)
            .InclusiveBetween(1, 31).WithMessage("Monthly due day must be between 1 and 31.");
    }
}
