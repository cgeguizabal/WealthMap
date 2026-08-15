using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.BankDefaults.DTOs;
using WealthMap.Domain.Entities;
using WealthMap.Domain.Enums;

namespace WealthMap.Application.Features.BankDefaults.Commands.UpsertBankDefault;

public class UpsertBankDefaultHandler : ICommandHandler<UpsertBankDefaultCommand, BankDefaultDto>
{
    private readonly IBankDefaultRepository _bankDefaults;
    private readonly IAccountRepository _accounts;
    private readonly IUnitOfWork _unitOfWork;

    public UpsertBankDefaultHandler(
        IBankDefaultRepository bankDefaults,
        IAccountRepository accounts,
        IUnitOfWork unitOfWork)
    {
        _bankDefaults = bankDefaults;
        _accounts = accounts;
        _unitOfWork = unitOfWork;
    }

    public async Task<BankDefaultDto> Handle(UpsertBankDefaultCommand request, CancellationToken ct)
    {
        // ExistsForUserAsync excludes archived accounts, which is exactly the check
        // wanted here: nominating an archived account as a fallback would create a
        // default that can never be honoured. Not the user's account → 404, same as
        // everywhere else.
        if (!await _accounts.ExistsForUserAsync(request.DefaultAccountId, request.UserId, ct))
            throw new NotFoundException("Account", request.DefaultAccountId);

        var direction = (TransferDirection)request.Direction;

        var existing = await _bankDefaults.GetForBankAsync(
            request.UserId, request.BankName, direction, ct);

        BankDefault bankDefault;

        if (existing is null)
        {
            bankDefault = new BankDefault(
                request.UserId, request.BankName, direction, request.DefaultAccountId);

            await _bankDefaults.AddAsync(bankDefault, ct);
        }
        else
        {
            existing.UpdateDefaultAccount(request.DefaultAccountId);
            _bankDefaults.Update(existing);
            bankDefault = existing;
        }

        await _unitOfWork.SaveChangesAsync(ct);

        var account = await _accounts.GetByIdForUserAsync(request.DefaultAccountId, request.UserId, ct);

        return BankDefaultDto.FromEntity(bankDefault, account?.Name ?? string.Empty);
    }
}
