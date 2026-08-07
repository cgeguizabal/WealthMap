using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;

namespace WealthMap.Application.Features.AdditionalIncomes.Commands.DeleteAdditionalIncome;

public class DeleteAdditionalIncomeHandler : ICommandHandler<DeleteAdditionalIncomeCommand, bool>
{
    private readonly IAdditionalIncomeRepository _incomes;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteAdditionalIncomeHandler(IAdditionalIncomeRepository incomes, IUnitOfWork unitOfWork)
    {
        _incomes = incomes;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteAdditionalIncomeCommand request, CancellationToken ct)
    {
        var income = await _incomes.GetByIdForUserAsync(request.Id, request.UserId, ct)
            ?? throw new NotFoundException("AdditionalIncome", request.Id);

        _incomes.Remove(income);
        await _unitOfWork.SaveChangesAsync(ct);

        return true;
    }
}