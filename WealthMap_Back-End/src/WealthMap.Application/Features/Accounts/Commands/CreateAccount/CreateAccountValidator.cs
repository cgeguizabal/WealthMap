using FluentValidation;
using WealthMap.Application.Common.Validation;
using WealthMap.Domain.Enums;

namespace WealthMap.Application.Features.Accounts.Commands.CreateAccount;

public class CreateAccountValidator : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Account name is required.")
            .MaximumLength(120);

        RuleFor(x => x.BankName)
            .NotEmpty().WithMessage("Bank name is required.")
            .MaximumLength(120);

        RuleFor(x => x.Type)
            .Must(t => Enum.IsDefined(typeof(AccountType), t))
            .WithMessage("Account type must be 1 (Checking) or 2 (Savings).");

        RuleFor(x => x.OpeningBalance)
            .GreaterThanOrEqualTo(0).WithMessage("Opening balance cannot be negative.");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .Length(3).WithMessage("Currency must be a 3-letter ISO code.");

        RuleFor(x => x.LastFour)
            .Must(TrackingRules.IsValidLastFour)
            .WithMessage(TrackingRules.LastFourMessage);

        RuleFor(x => x.TrackingMode)
            .Must(m => m is null || TrackingRules.IsDefinedMode(m.Value))
            .WithMessage(TrackingRules.TrackingModeMessage);

        // Keyed to lastFour rather than trackingMode: the digits are what is missing,
        // so that is the field the form should mark.
        RuleFor(x => x.LastFour)
            .Must((command, lastFour) =>
                command.TrackingMode is null || TrackingRules.IsIdentifiable(command.TrackingMode.Value, lastFour))
            .WithMessage(TrackingRules.SyncNeedsLastFourMessage);

        RuleFor(x => x.DebitCardType)
            .Must(t => t is null || Enum.IsDefined(typeof(DebitCardType), t.Value))
            .WithMessage(TrackingRules.DebitCardTypeMessage);

        // Optional even when a card exists: a user may know they have one without
        // knowing its number, and the digits are only ever identifying data.
        RuleFor(x => x.DebitCardLastFour)
            .Must(TrackingRules.IsValidLastFour)
            .WithMessage(TrackingRules.LastFourMessage);
    }
}