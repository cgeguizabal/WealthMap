using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Debts.DTOs;

namespace WealthMap.Application.Features.Debts.Queries.GetDebts;

public record GetDebtsQuery(Guid UserId) : IQuery<IReadOnlyList<DebtDto>>;
