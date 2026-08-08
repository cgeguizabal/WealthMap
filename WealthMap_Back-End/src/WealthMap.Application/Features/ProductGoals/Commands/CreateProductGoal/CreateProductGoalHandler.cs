using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.ProductGoals.DTOs;
using WealthMap.Domain.Entities;
using WealthMap.Domain.ValueObjects;

namespace WealthMap.Application.Features.ProductGoals.Commands.CreateProductGoal;

public class CreateProductGoalHandler : ICommandHandler<CreateProductGoalCommand, ProductGoalDto>
{
    private readonly IProductGoalRepository _goals;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProductGoalHandler(IProductGoalRepository goals, IUnitOfWork unitOfWork)
    {
        _goals = goals;
        _unitOfWork = unitOfWork;
    }

    public async Task<ProductGoalDto> Handle(CreateProductGoalCommand request, CancellationToken ct)
    {
        var goal = new ProductGoal(
            request.UserId,
            request.Name,
            new Money(request.TargetAmount, request.Currency),
            new Money(request.CurrentAmount ?? 0, request.Currency),
            request.Deadline);

        await _goals.AddAsync(goal, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return ProductGoalDto.FromEntity(goal);
    }
}
