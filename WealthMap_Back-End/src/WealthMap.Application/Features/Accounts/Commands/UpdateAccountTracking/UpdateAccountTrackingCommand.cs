using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Accounts.DTOs;

namespace WealthMap.Application.Features.Accounts.Commands.UpdateAccountTracking;

/// <summary>
/// Sets both tracking fields together, because they constrain each other: which of
/// the two is written first decides whether a valid pair is accepted.
/// </summary>
public record UpdateAccountTrackingCommand(
    Guid Id,
    Guid UserId,
    int TrackingMode,
    string? LastFour) : ICommand<AccountDto>;
