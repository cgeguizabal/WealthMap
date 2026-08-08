using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.ProductGoals.DTOs;
using WealthMap.Domain.ValueObjects;

namespace WealthMap.Application.Features.ProductGoals.Commands.UpdateProductGoal;

public class UpdateProductGoalHandler : ICommandHandler<UpdateProductGoalCommand, ProductGoalDto>
{
    private readonly IProductGoalRepository _goals;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProductGoalHandler(IProductGoalRepository goals, IUnitOfWork unitOfWork)
    {
        _goals = goals;
        _unitOfWork = unitOfWork;
    }

    public async Task<ProductGoalDto> Handle(UpdateProductGoalCommand request, CancellationToken ct)
    {
        var goal = await _goals.GetByIdForUserAsync(request.Id, request.UserId, ct)
            ?? throw new NotFoundException("ProductGoal", request.Id);

        goal.UpdateDetails(
            request.Name,
            new Money(request.TargetAmount, goal.TargetAmount.Currency),
            request.Deadline);

        await _unitOfWork.SaveChangesAsync(ct);

        return ProductGoalDto.FromEntity(goal);
    }
}
