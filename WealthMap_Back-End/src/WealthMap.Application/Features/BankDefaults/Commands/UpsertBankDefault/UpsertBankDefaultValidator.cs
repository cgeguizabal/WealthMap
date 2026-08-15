using FluentValidation;
using WealthMap.Domain.Enums;

namespace WealthMap.Application.Features.BankDefaults.Commands.UpsertBankDefault;

public class UpsertBankDefaultValidator : AbstractValidator<UpsertBankDefaultCommand>
{
    public UpsertBankDefaultValidator()
    {
        RuleFor(x => x.BankName)
            .NotEmpty().WithMessage("Bank name is required.")
            .MaximumLength(120);

        RuleFor(x => x.Direction)
            .Must(d => Enum.IsDefined(typeof(TransferDirection), d))
            .WithMessage("Direction must be 1 (Inbound) or 2 (Outbound).");

        RuleFor(x => x.DefaultAccountId)
            .NotEmpty().WithMessage("A default account is required.");
    }
}
