using FluentValidation;

namespace WealthMap.Application.Features.Accounts.Commands.TransferBetweenAccounts;

public class TransferBetweenAccountsValidator : AbstractValidator<TransferBetweenAccountsCommand>
{
    public TransferBetweenAccountsValidator()
    {
        RuleFor(x => x.FromAccountId)
            .NotEmpty().WithMessage("Source account is required.");

        RuleFor(x => x.ToAccountId)
            .NotEmpty().WithMessage("Destination account is required.")
            .NotEqual(x => x.FromAccountId)
            .WithMessage("Cannot transfer to the same account.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Transfer amount must be greater than zero.");

        RuleFor(x => x.Description)
            .MaximumLength(250).When(x => x.Description is not null);
    }
}