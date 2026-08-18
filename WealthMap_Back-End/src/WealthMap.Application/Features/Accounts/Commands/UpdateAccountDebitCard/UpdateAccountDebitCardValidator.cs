using FluentValidation;
using WealthMap.Application.Common.Validation;
using WealthMap.Domain.Enums;

namespace WealthMap.Application.Features.Accounts.Commands.UpdateAccountDebitCard;

public class UpdateAccountDebitCardValidator : AbstractValidator<UpdateAccountDebitCardCommand>
{
    public UpdateAccountDebitCardValidator()
    {
        RuleFor(x => x.DebitCardType)
            .Must(t => Enum.IsDefined(typeof(DebitCardType), t))
            .WithMessage(TrackingRules.DebitCardTypeMessage);

        // Optional even when a card exists: a user may know they have one without
        // knowing its number, and the digits are only ever identifying data.
        RuleFor(x => x.DebitCardLastFour)
            .Must(TrackingRules.IsValidLastFour)
            .WithMessage(TrackingRules.LastFourMessage);
    }
}
