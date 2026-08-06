using FluentValidation;

namespace WealthMap.Application.Features.Accounts.Commands.UpdateAccount;

public class UpdateAccountValidator : AbstractValidator<UpdateAccountCommand>
{
    public UpdateAccountValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Account name is required.")
            .MaximumLength(120);

        RuleFor(x => x.BankName)
            .NotEmpty().WithMessage("Bank name is required.")
            .MaximumLength(120);

        RuleFor(x => x.Notes)
            .MaximumLength(500).When(x => x.Notes is not null);
    }
}