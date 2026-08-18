using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Debts.DTOs;

namespace WealthMap.Application.Features.Debts.Commands.MarkDebtDefaulted;

public class MarkDebtDefaultedHandler : ICommandHandler<MarkDebtDefaultedCommand, DebtDto>
{
    private readonly IDebtRepository _debts;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserClock _clock;

    public MarkDebtDefaultedHandler(IDebtRepository debts, IUnitOfWork unitOfWork,
        IUserClock clock)
    {
        _debts = debts;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<DebtDto> Handle(MarkDebtDefaultedCommand request, CancellationToken ct)
    {
        var debt = await _debts.GetByIdForUserAsync(request.Id, request.UserId, ct)
            ?? throw new NotFoundException("Debt", request.Id);

        debt.MarkDefaulted();

        await _unitOfWork.SaveChangesAsync(ct);

        return DebtDto.FromEntity(debt, _clock.Today);
    }
}
