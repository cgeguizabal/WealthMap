using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Jobs.DTOs;

namespace WealthMap.Application.Features.Jobs.Queries.GetSalaryDeposits;

public record GetSalaryDepositsQuery(Guid JobId, Guid UserId)
    : IQuery<IReadOnlyList<SalaryDepositDto>>;
