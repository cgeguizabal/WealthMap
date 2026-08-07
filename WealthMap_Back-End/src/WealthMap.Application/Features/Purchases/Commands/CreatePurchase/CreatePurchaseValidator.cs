using FluentValidation;
using WealthMap.Domain.Enums;

namespace WealthMap.Application.Features.Purchases.Commands.CreatePurchase;

public class CreatePurchaseValidator : AbstractValidator<CreatePurchaseCommand>
{
    public CreatePurchaseValidator()
    {
        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(200);

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Purchase amount must be greater than zero.");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required.")
            .MaximumLength(80);

        RuleFor(x => x.PaymentMethod)
            .Must(m => Enum.IsDefined(typeof(PaymentMethod), m))
            .WithMessage("Payment method must be 1 (DebitAccount), 2 (CreditCard) or 3 (Cash).");

        RuleFor(x => x.AccountId)
            .NotEmpty().WithMessage("A debit purchase requires an account.")
            .When(x => x.PaymentMethod == (int)PaymentMethod.DebitAccount);

        RuleFor(x => x.CreditCardId)
            .NotEmpty().WithMessage("A credit purchase requires a credit card.")
            .When(x => x.PaymentMethod == (int)PaymentMethod.CreditCard);

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("A cash purchase requires an explicit currency.")
            .Length(3).WithMessage("Currency must be a 3-letter ISO code.")
            .When(x => x.PaymentMethod == (int)PaymentMethod.Cash);

        RuleFor(x => x.OccurredAt)
            .Must(d => d!.Value <= DateTime.UtcNow.AddMinutes(5))
            .WithMessage("Purchase date cannot be in the future.")
            .When(x => x.OccurredAt is not null);

        RuleFor(x => x.Notes)
            .MaximumLength(500).When(x => x.Notes is not null);
    }
}
