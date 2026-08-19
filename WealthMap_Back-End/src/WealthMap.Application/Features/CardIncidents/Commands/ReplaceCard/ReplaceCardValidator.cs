using FluentValidation;
using WealthMap.Application.Common.Validation;

namespace WealthMap.Application.Features.CardIncidents.Commands.ReplaceCard;

public class ReplaceCardValidator : AbstractValidator<ReplaceCardCommand>
{
    public ReplaceCardValidator()
    {
        RuleFor(x => x.Kind)
            .IsInEnum()
            .WithMessage("Card must be 1 (CreditCard) or 2 (DebitCard).");

        RuleFor(x => x.NewLastFour)
            .Must(TrackingRules.IsValidLastFour)
            .WithMessage(TrackingRules.LastFourMessage);

        RuleFor(x => x.ReplacedOn)
            .Must(date => date is null || date <= DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)))
            .WithMessage("A replacement cannot arrive on a future date.");

        RuleFor(x => x.Notes)
            .MaximumLength(500);
    }
}
