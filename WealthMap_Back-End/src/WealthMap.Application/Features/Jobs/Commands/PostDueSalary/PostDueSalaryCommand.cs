using WealthMap.Application.Common.Messaging;

namespace WealthMap.Application.Features.Jobs.Commands.PostDueSalary;

/// <summary>
/// Settles this job's unpaid paydays now instead of waiting for the daily run.
/// Safe to call repeatedly — already-settled paydays are skipped.
/// </summary>
public record PostDueSalaryCommand(Guid JobId, Guid UserId) : ICommand<int>;
