using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Debts.DTOs;
using WealthMap.Domain.Entities;
using WealthMap.Domain.ValueObjects;

namespace WealthMap.Application.Features.Debts.Commands.CreateDebt;

public class CreateDebtHandler : ICommandHandler<CreateDebtCommand, DebtDto>
{
    private readonly IDebtRepository _debts;
    private readonly IUnitOfWork _unitOfWork;

    public CreateDebtHandler(IDebtRepository debts, IUnitOfWork unitOfWork)
    {
        _debts = debts;
        _unitOfWork = unitOfWork;
    }

    public async Task<DebtDto> Handle(CreateDebtCommand request, CancellationToken ct)
    {
        // RemainingAmount defaults to the original: a brand-new debt is unpaid.
        // Passing a lower value registers a debt that was already partially paid.
        var debt = new Debt(
            request.UserId,
            request.Name,
            new Money(request.OriginalAmount, request.Currency),
            new Money(request.RemainingAmount ?? request.OriginalAmount, request.Currency),
            new Money(request.MonthlyPayment, request.Currency),
            request.MonthlyDueDay);

        await _debts.AddAsync(debt, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return DebtDto.FromEntity(debt);
    }
}
