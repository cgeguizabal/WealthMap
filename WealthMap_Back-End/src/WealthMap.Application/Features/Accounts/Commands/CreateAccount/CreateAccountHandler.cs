using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Accounts.DTOs;
using WealthMap.Domain.Entities;
using WealthMap.Domain.Enums;
using WealthMap.Domain.ValueObjects;

namespace WealthMap.Application.Features.Accounts.Commands.CreateAccount;

public class CreateAccountHandler : ICommandHandler<CreateAccountCommand, AccountDto>
{
    private readonly IAccountRepository _accounts;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAccountHandler(IAccountRepository accounts, IUnitOfWork unitOfWork)
    {
        _accounts = accounts;
        _unitOfWork = unitOfWork;
    }

    public async Task<AccountDto> Handle(CreateAccountCommand request, CancellationToken ct)
    {
        var openingBalance = new Money(request.OpeningBalance, request.Currency);

        var account = new Account(
            request.UserId,
            request.Name,
            request.BankName,
            (AccountType)request.Type,
            openingBalance);

        await _accounts.AddAsync(account, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return AccountDto.FromEntity(account);
    }
}