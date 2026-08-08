using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;

namespace WealthMap.Application.Features.SavingsGoals.Commands.DeleteSavingsGoal;

public class DeleteSavingsGoalHandler : ICommandHandler<DeleteSavingsGoalCommand, bool>
{
    private readonly ISavingsGoalRepository _goals;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteSavingsGoalHandler(ISavingsGoalRepository goals, IUnitOfWork unitOfWork)
    {
        _goals = goals;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteSavingsGoalCommand request, CancellationToken ct)
    {
        var goal = await _goals.GetByIdForUserAsync(request.Id, request.UserId, ct)
            ?? throw new NotFoundException("SavingsGoal", request.Id);

        _goals.Remove(goal);
        await _unitOfWork.SaveChangesAsync(ct);

        return true;
    }
}
