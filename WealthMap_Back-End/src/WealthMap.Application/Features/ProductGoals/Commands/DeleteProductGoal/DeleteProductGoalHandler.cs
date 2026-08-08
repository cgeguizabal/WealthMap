using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;

namespace WealthMap.Application.Features.ProductGoals.Commands.DeleteProductGoal;

public class DeleteProductGoalHandler : ICommandHandler<DeleteProductGoalCommand, bool>
{
    private readonly IProductGoalRepository _goals;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteProductGoalHandler(IProductGoalRepository goals, IUnitOfWork unitOfWork)
    {
        _goals = goals;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteProductGoalCommand request, CancellationToken ct)
    {
        var goal = await _goals.GetByIdForUserAsync(request.Id, request.UserId, ct)
            ?? throw new NotFoundException("ProductGoal", request.Id);

        _goals.Remove(goal);
        await _unitOfWork.SaveChangesAsync(ct);

        return true;
    }
}
