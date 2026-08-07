using FluentValidation;

namespace WealthMap.Application.Features.InstallmentPurchases.Commands.CreateInstallmentPurchase;

public class CreateInstallmentPurchaseValidator : AbstractValidator<CreateInstallmentPurchaseCommand>
{
    public CreateInstallmentPurchaseValidator()
    {
        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(200);

        RuleFor(x => x.TotalPrice)
            .GreaterThan(0).WithMessage("Total price must be greater than zero.");

        RuleFor(x => x.CreditCardId)
            .NotEmpty().WithMessage("A credit card is required.");

        RuleFor(x => x.MonthsCount)
            .InclusiveBetween(1, 120).WithMessage("Months must be between 1 and 120.");

        RuleFor(x => x.PurchasedAt)
            .Must(d => d!.Value <= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Purchase date cannot be in the future.")
            .When(x => x.PurchasedAt is not null);
    }
}
