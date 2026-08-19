using FluentValidation;
using WealthMap.Domain.Enums;

namespace WealthMap.Application.Features.CardIncidents.Commands.ReportCardLost;

public class ReportCardLostValidator : AbstractValidator<ReportCardLostCommand>
{
    public ReportCardLostValidator()
    {
        RuleFor(x => x.Kind)
            .IsInEnum()
            .WithMessage("Card must be 1 (CreditCard) or 2 (DebitCard).");

        RuleFor(x => x.Reason)
            .IsInEnum()
            .WithMessage("Reason must be 1 (Lost), 2 (Stolen), 3 (Damaged) or 4 (Compromised).");

        // A future date is a typo rather than a report: a card cannot be stolen
        // tomorrow. The past stays open, because people notice days later.
        //
        // Tomorrow, not today, because a validator has no time zone. A user ahead
        // of UTC reporting a card this evening would otherwise be told their own
        // date is in the future. The slack costs nothing: this exists to catch a
        // mistyped year, not to police the hour.
        RuleFor(x => x.ReportedOn)
            .Must(date => date is null || date <= DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)))
            .WithMessage("A card cannot be reported for a future date.");

        RuleFor(x => x.Notes)
            .MaximumLength(500);
    }
}
