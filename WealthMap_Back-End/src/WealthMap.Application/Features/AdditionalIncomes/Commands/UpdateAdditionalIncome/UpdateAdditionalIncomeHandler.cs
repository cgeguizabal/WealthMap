using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.AdditionalIncomes.DTOs;
using WealthMap.Domain.Enums;
using WealthMap.Domain.ValueObjects;

namespace WealthMap.Application.Features.AdditionalIncomes.Commands.UpdateAdditionalIncome;

public class UpdateAdditionalIncomeHandler : ICommandHandler<UpdateAdditionalIncomeCommand, AdditionalIncomeDto>
{
    private readonly IAdditionalIncomeRepository _incomes;
    private readonly IAccountRepository _accounts;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAdditionalIncomeHandler(
        IAdditionalIncomeRepository incomes,
        IAccountRepository accounts,
        IUnitOfWork unitOfWork)
    {
        _incomes = incomes;
        _accounts = accounts;
        _unitOfWork = unitOfWork;
    }

    public async Task<AdditionalIncomeDto> Handle(UpdateAdditionalIncomeCommand request, CancellationToken ct)
    {
        var income = await _incomes.GetByIdForUserAsync(request.Id, request.UserId, ct)
            ?? throw new NotFoundException("AdditionalIncome", request.Id);

        if (!await _accounts.ExistsForUserAsync(request.DepositAccountId, request.UserId, ct))
            throw new NotFoundException("Account", request.DepositAccountId);

        income.Update(
            request.Name,
            new Money(request.Amount, income.Amount.Currency),
            (IncomeFrequency)request.Frequency,
            request.DepositAccountId);

        await _unitOfWork.SaveChangesAsync(ct);

        return AdditionalIncomeDto.FromEntity(income);
    }
}