using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.ProductGoals.DTOs;
using WealthMap.Domain.ValueObjects;

namespace WealthMap.Application.Features.ProductGoals.Commands.ContributeToProductGoal;

public class ContributeToProductGoalHandler : ICommandHandler<ContributeToProductGoalCommand, ProductGoalDto>
{
    private readonly IProductGoalRepository _goals;
    private readonly IUnitOfWork _unitOfWork;

    public ContributeToProductGoalHandler(IProductGoalRepository goals, IUnitOfWork unitOfWork)
    {
        _goals = goals;
        _unitOfWork = unitOfWork;
    }

    public async Task<ProductGoalDto> Handle(ContributeToProductGoalCommand request, CancellationToken ct)
    {
        var goal = await _goals.GetByIdForUserAsync(request.GoalId, request.UserId, ct)
            ?? throw new NotFoundException("ProductGoal", request.GoalId);

        goal.Contribute(new Money(request.Amount, goal.TargetAmount.Currency));

        await _unitOfWork.SaveChangesAsync(ct);

        return ProductGoalDto.FromEntity(goal);
    }
}
