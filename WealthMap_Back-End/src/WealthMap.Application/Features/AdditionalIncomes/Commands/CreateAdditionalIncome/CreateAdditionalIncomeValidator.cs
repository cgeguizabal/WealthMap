using FluentValidation;
using WealthMap.Domain.Enums;

namespace WealthMap.Application.Features.AdditionalIncomes.Commands.CreateAdditionalIncome;

public class CreateAdditionalIncomeValidator : AbstractValidator<CreateAdditionalIncomeCommand>
{
    public CreateAdditionalIncomeValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Income name is required.")
            .MaximumLength(120);

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Income amount must be greater than zero.");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .Length(3).WithMessage("Currency must be a 3-letter ISO code.");

        RuleFor(x => x.Frequency)
            .Must(f => Enum.IsDefined(typeof(IncomeFrequency), f))
            .WithMessage("Frequency must be 1 (Weekly), 2 (Biweekly), 3 (Monthly) or 4 (Yearly).");

        RuleFor(x => x.DepositAccountId)
            .NotEmpty().WithMessage("Deposit account is required.");
    }
}