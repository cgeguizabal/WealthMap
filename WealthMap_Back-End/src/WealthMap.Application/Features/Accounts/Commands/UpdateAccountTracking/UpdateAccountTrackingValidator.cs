using FluentValidation;
using WealthMap.Application.Common.Validation;

namespace WealthMap.Application.Features.Accounts.Commands.UpdateAccountTracking;

public class UpdateAccountTrackingValidator : AbstractValidator<UpdateAccountTrackingCommand>
{
    public UpdateAccountTrackingValidator()
    {
        RuleFor(x => x.TrackingMode)
            .Must(TrackingRules.IsDefinedMode)
            .WithMessage(TrackingRules.TrackingModeMessage);

        RuleFor(x => x.LastFour)
            .Must(TrackingRules.IsValidLastFour)
            .WithMessage(TrackingRules.LastFourMessage);

        // Keyed to lastFour rather than trackingMode: the digits are what is missing,
        // so that is the field the form should mark.
        RuleFor(x => x.LastFour)
            .Must((command, lastFour) => TrackingRules.IsIdentifiable(command.TrackingMode, lastFour))
            .WithMessage(TrackingRules.SyncNeedsLastFourMessage);
    }
}
