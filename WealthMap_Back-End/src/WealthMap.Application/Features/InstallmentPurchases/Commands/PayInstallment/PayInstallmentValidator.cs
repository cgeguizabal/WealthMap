using FluentValidation;

namespace WealthMap.Application.Features.InstallmentPurchases.Commands.PayInstallment;

public class PayInstallmentValidator : AbstractValidator<PayInstallmentCommand>
{
    private static readonly string[] AllowedSources = ["Account", "External"];

    public PayInstallmentValidator()
    {
        RuleFor(x => x.SourceType)
            .Must(s => AllowedSources.Contains(s, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Source type must be 'Account' or 'External'.");

        RuleFor(x => x.SourceAccountId)
            .NotEmpty().WithMessage("Source account is required when paying from an account.")
            .When(x => string.Equals(x.SourceType, "Account", StringComparison.OrdinalIgnoreCase));

        RuleFor(x => x.SourceAccountId)
            .Empty().WithMessage("Source account must not be set for an external payment.")
            .When(x => string.Equals(x.SourceType, "External", StringComparison.OrdinalIgnoreCase));
    }
}
