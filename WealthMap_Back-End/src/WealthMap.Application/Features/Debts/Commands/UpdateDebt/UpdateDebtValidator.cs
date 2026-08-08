using FluentValidation;

namespace WealthMap.Application.Features.Debts.Commands.UpdateDebt;

public class UpdateDebtValidator : AbstractValidator<UpdateDebtCommand>
{
    public UpdateDebtValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Debt name is required.")
            .MaximumLength(120);

        RuleFor(x => x.MonthlyPayment)
            .GreaterThan(0).WithMessage("Monthly payment must be greater than zero.");

        RuleFor(x => x.MonthlyDueDay)
            .InclusiveBetween(1, 31).WithMessage("Monthly due day must be between 1 and 31.");
    }
}
