using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Debts.DTOs;
using WealthMap.Domain.ValueObjects;

namespace WealthMap.Application.Features.Debts.Commands.UpdateDebt;

public class UpdateDebtHandler : ICommandHandler<UpdateDebtCommand, DebtDto>
{
    private readonly IDebtRepository _debts;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserClock _clock;

    public UpdateDebtHandler(IDebtRepository debts, IUnitOfWork unitOfWork,
        IUserClock clock)
    {
        _debts = debts;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<DebtDto> Handle(UpdateDebtCommand request, CancellationToken ct)
    {
        var debt = await _debts.GetByIdForUserAsync(request.Id, request.UserId, ct)
            ?? throw new NotFoundException("Debt", request.Id);

        debt.UpdateDetails(
            request.Name,
            new Money(request.MonthlyPayment, debt.OriginalAmount.Currency),
            request.MonthlyDueDay);

        await _unitOfWork.SaveChangesAsync(ct);

        return DebtDto.FromEntity(debt, _clock.Today);
    }
}
