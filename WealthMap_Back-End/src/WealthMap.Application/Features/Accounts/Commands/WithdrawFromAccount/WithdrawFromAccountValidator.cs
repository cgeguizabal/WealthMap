using FluentValidation;

namespace WealthMap.Application.Features.Accounts.Commands.WithdrawFromAccount;

public class WithdrawFromAccountValidator : AbstractValidator<WithdrawFromAccountCommand>
{
    public WithdrawFromAccountValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Withdrawal amount must be greater than zero.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(250);

        RuleFor(x => x.Location)
            .MaximumLength(200).When(x => x.Location is not null);
    }
}