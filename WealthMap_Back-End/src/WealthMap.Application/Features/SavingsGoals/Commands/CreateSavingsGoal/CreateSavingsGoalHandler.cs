using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.SavingsGoals.DTOs;
using WealthMap.Domain.Entities;
using WealthMap.Domain.Enums;
using WealthMap.Domain.Exceptions;
using WealthMap.Domain.ValueObjects;

namespace WealthMap.Application.Features.SavingsGoals.Commands.CreateSavingsGoal;

public class CreateSavingsGoalHandler : ICommandHandler<CreateSavingsGoalCommand, SavingsGoalDto>
{
    private readonly ISavingsGoalRepository _goals;
    private readonly IAccountRepository _accounts;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSavingsGoalHandler(
        ISavingsGoalRepository goals,
        IAccountRepository accounts,
        IUnitOfWork unitOfWork)
    {
        _goals = goals;
        _accounts = accounts;
        _unitOfWork = unitOfWork;
    }

    public async Task<SavingsGoalDto> Handle(CreateSavingsGoalCommand request, CancellationToken ct)
    {
        if (request.LinkedAccountId is not null)
        {
            var account = await _accounts.GetByIdForUserAsync(request.LinkedAccountId.Value, request.UserId, ct)
                ?? throw new NotFoundException("Account", request.LinkedAccountId.Value);

            if (account.Type != AccountType.Savings)
                throw new DomainException("A goal can only be linked to a savings account.");

            if (account.Balance.Currency != request.Currency.ToUpperInvariant())
                throw new DomainException("The linked account must use the goal's currency.");
        }

        var goal = new SavingsGoal(
            request.UserId,
            request.Name,
            new Money(request.TargetAmount, request.Currency),
            new Money(request.CurrentAmount ?? 0, request.Currency),
            request.Deadline,
            request.LinkedAccountId);

        await _goals.AddAsync(goal, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return SavingsGoalDto.FromEntity(goal);
    }
}
