using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Accounts.DTOs;

namespace WealthMap.Application.Features.Accounts.Queries.GetAccountById;

public class GetAccountByIdHandler : IQueryHandler<GetAccountByIdQuery, AccountDto>
{
    private readonly IAccountRepository _accounts;

    public GetAccountByIdHandler(IAccountRepository accounts) => _accounts = accounts;

    public async Task<AccountDto> Handle(GetAccountByIdQuery request, CancellationToken ct)
    {
        var account = await _accounts.GetByIdForUserAsync(request.Id, request.UserId, ct)
            ?? throw new NotFoundException("Account", request.Id);

        return AccountDto.FromEntity(account);
    }
}