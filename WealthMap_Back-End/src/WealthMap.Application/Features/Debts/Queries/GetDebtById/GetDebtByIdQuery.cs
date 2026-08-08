using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Debts.DTOs;

namespace WealthMap.Application.Features.Debts.Queries.GetDebtById;

public record GetDebtByIdQuery(Guid Id, Guid UserId) : IQuery<DebtDto>;
