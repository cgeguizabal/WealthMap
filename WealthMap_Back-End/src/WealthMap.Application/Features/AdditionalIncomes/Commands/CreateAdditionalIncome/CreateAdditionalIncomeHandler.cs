using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.AdditionalIncomes.DTOs;
using WealthMap.Domain.Entities;
using WealthMap.Domain.Enums;
using WealthMap.Domain.ValueObjects;

namespace WealthMap.Application.Features.AdditionalIncomes.Commands.CreateAdditionalIncome;

public class CreateAdditionalIncomeHandler : ICommandHandler<CreateAdditionalIncomeCommand, AdditionalIncomeDto>
{
    private readonly IAdditionalIncomeRepository _incomes;
    private readonly IAccountRepository _accounts;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAdditionalIncomeHandler(
        IAdditionalIncomeRepository incomes,
        IAccountRepository accounts,
        IUnitOfWork unitOfWork)
    {
        _incomes = incomes;
        _accounts = accounts;
        _unitOfWork = unitOfWork;
    }

    public async Task<AdditionalIncomeDto> Handle(CreateAdditionalIncomeCommand request, CancellationToken ct)
    {
        if (!await _accounts.ExistsForUserAsync(request.DepositAccountId, request.UserId, ct))
            throw new NotFoundException("Account", request.DepositAccountId);

        var income = new AdditionalIncome(
            request.UserId,
            request.Name,
            new Money(request.Amount, request.Currency),
            (IncomeFrequency)request.Frequency,
            request.DepositAccountId);

        await _incomes.AddAsync(income, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return AdditionalIncomeDto.FromEntity(income);
    }
}