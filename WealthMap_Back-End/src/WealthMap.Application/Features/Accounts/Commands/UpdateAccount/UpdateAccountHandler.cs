using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Accounts.DTOs;

namespace WealthMap.Application.Features.Accounts.Commands.UpdateAccount;

public class UpdateAccountHandler : ICommandHandler<UpdateAccountCommand, AccountDto>
{
    private readonly IAccountRepository _accounts;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAccountHandler(IAccountRepository accounts, IUnitOfWork unitOfWork)
    {
        _accounts = accounts;
        _unitOfWork = unitOfWork;
    }

    public async Task<AccountDto> Handle(UpdateAccountCommand request, CancellationToken ct)
    {
        var account = await _accounts.GetByIdForUserAsync(request.Id, request.UserId, ct)
            ?? throw new NotFoundException("Account", request.Id);

        account.UpdateDetails(request.Name, request.BankName, request.Notes);

        await _unitOfWork.SaveChangesAsync(ct);

        return AccountDto.FromEntity(account);
    }
}