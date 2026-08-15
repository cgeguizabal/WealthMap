using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.BankDefaults.DTOs;

namespace WealthMap.Application.Features.BankDefaults.Queries.GetBankDefaults;

public record GetBankDefaultsQuery(Guid UserId) : IQuery<IReadOnlyList<BankDefaultDto>>;
