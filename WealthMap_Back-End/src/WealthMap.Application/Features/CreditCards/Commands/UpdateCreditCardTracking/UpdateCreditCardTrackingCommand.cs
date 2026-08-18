using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.CreditCards.DTOs;

namespace WealthMap.Application.Features.CreditCards.Commands.UpdateCreditCardTracking;

/// <summary>
/// Sets both tracking fields together, because they constrain each other: which of
/// the two is written first decides whether a valid pair is accepted.
/// </summary>
public record UpdateCreditCardTrackingCommand(
    Guid Id,
    Guid UserId,
    int TrackingMode,
    string? LastFour) : ICommand<CreditCardDto>;
