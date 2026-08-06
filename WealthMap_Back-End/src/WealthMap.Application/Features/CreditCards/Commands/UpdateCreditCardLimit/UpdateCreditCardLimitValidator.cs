using FluentValidation;

namespace WealthMap.Application.Features.CreditCards.Commands.UpdateCreditCardLimit;

public class UpdateCreditCardLimitValidator : AbstractValidator<UpdateCreditCardLimitCommand>
{
    public UpdateCreditCardLimitValidator()
    {
        RuleFor(x => x.NewLimit)
            .GreaterThan(0).WithMessage("Credit limit must be greater than zero.");
    }
}