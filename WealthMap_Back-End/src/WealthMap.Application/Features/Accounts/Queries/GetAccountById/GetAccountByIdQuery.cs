using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Accounts.DTOs;

namespace WealthMap.Application.Features.Accounts.Queries.GetAccountById;

public record GetAccountByIdQuery(Guid Id, Guid UserId) : IQuery<AccountDto>;