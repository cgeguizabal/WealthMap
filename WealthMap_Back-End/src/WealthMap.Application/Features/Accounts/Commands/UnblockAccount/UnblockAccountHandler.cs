using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Accounts.DTOs;

namespace WealthMap.Application.Features.Accounts.Commands.UnblockAccount;

public class UnblockAccountHandler : ICommandHandler<UnblockAccountCommand, AccountDto>
{
    private readonly IAccountRepository _accounts;
    private readonly IUnitOfWork _unitOfWork;

    public UnblockAccountHandler(IAccountRepository accounts, IUnitOfWork unitOfWork)
    {
        _accounts = accounts;
        _unitOfWork = unitOfWork;
    }

    public async Task<AccountDto> Handle(UnblockAccountCommand request, CancellationToken ct)
    {
        var account = await _accounts.GetByIdForUserAsync(request.Id, request.UserId, ct)
            ?? throw new NotFoundException("Account", request.Id);

        account.UnblockForSaving();

        await _unitOfWork.SaveChangesAsync(ct);

        return AccountDto.FromEntity(account);
    }
}