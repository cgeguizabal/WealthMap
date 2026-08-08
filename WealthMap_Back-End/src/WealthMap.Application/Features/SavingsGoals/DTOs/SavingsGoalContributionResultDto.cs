using WealthMap.Application.Features.Accounts.DTOs;

namespace WealthMap.Application.Features.SavingsGoals.DTOs;

public record SavingsGoalContributionResultDto(
    SavingsGoalDto Goal,
    AccountMovementDto? SourceMovement);
