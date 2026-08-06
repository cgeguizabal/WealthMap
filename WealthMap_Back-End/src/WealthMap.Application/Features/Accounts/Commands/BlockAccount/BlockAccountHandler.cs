using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Accounts.DTOs;

namespace WealthMap.Application.Features.Accounts.Commands.BlockAccount;

public class BlockAccountHandler : ICommandHandler<BlockAccountCommand, AccountDto>
{
    private readonly IAccountRepository _accounts;
    private readonly IUnitOfWork _unitOfWork;

    public BlockAccountHandler(IAccountRepository accounts, IUnitOfWork unitOfWork)
    {
        _accounts = accounts;
        _unitOfWork = unitOfWork;
    }

    public async Task<AccountDto> Handle(BlockAccountCommand request, CancellationToken ct)
    {
        var account = await _accounts.GetByIdForUserAsync(request.Id, request.UserId, ct)
            ?? throw new NotFoundException("Account", request.Id);

        account.BlockForSaving();

        await _unitOfWork.SaveChangesAsync(ct);

        return AccountDto.FromEntity(account);
    }
}