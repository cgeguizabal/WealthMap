using FluentValidation;

namespace WealthMap.Application.Features.CardIncidents.Commands.MarkCardRecovered;

public class MarkCardRecoveredValidator : AbstractValidator<MarkCardRecoveredCommand>
{
    public MarkCardRecoveredValidator()
    {
        RuleFor(x => x.Kind)
            .IsInEnum()
            .WithMessage("Card must be 1 (CreditCard) or 2 (DebitCard).");

        RuleFor(x => x.RecoveredOn)
            .Must(date => date is null || date <= DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)))
            .WithMessage("A card cannot be found on a future date.");

        RuleFor(x => x.Notes)
            .MaximumLength(500);
    }
}
