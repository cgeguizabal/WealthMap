using FluentValidation;
using WealthMap.Domain.Enums;

namespace WealthMap.Application.Features.Accounts.Commands.DepositToAccount;

public class DepositToAccountValidator : AbstractValidator<DepositToAccountCommand>
{
    public DepositToAccountValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Deposit amount must be greater than zero.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(250);

        RuleFor(x => x.Type)
            .Must(t => t is (int)MovementType.Deposit or (int)MovementType.Bonus)
            .WithMessage("Type must be 2 (Deposit) or 3 (Bonus). Other inbound types are system-generated.");
    }
}