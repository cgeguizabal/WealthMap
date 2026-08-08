using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;

namespace WealthMap.Application.Features.Debts.Commands.DeleteDebt;

public class DeleteDebtHandler : ICommandHandler<DeleteDebtCommand, bool>
{
    private readonly IDebtRepository _debts;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteDebtHandler(IDebtRepository debts, IUnitOfWork unitOfWork)
    {
        _debts = debts;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteDebtCommand request, CancellationToken ct)
    {
        var debt = await _debts.GetByIdForUserAsync(request.Id, request.UserId, ct)
            ?? throw new NotFoundException("Debt", request.Id);

        _debts.Remove(debt);
        await _unitOfWork.SaveChangesAsync(ct);

        return true;
    }
}
