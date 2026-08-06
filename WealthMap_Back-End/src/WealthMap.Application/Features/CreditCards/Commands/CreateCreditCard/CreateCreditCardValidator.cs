using FluentValidation;

namespace WealthMap.Application.Features.CreditCards.Commands.CreateCreditCard;

public class CreateCreditCardValidator : AbstractValidator<CreateCreditCardCommand>
{
    public CreateCreditCardValidator()
    {
        RuleFor(x => x.CardName)
            .NotEmpty().WithMessage("Card name is required.")
            .MaximumLength(120);

        RuleFor(x => x.BankName)
            .NotEmpty().WithMessage("Bank name is required.")
            .MaximumLength(120);

        RuleFor(x => x.CreditLimit)
            .GreaterThan(0).WithMessage("Credit limit must be greater than zero.");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .Length(3).WithMessage("Currency must be a 3-letter ISO code.");

        RuleFor(x => x.AnnualInterestRate)
            .InclusiveBetween(0, 200).WithMessage("Annual interest rate must be between 0 and 200.");

        RuleFor(x => x.PaymentDueDay)
            .InclusiveBetween(1, 31).WithMessage("Payment due day must be between 1 and 31.");

        RuleFor(x => x.StatementCutoffDay)
            .InclusiveBetween(1, 31).WithMessage("Statement cutoff day must be between 1 and 31.");
    }
}