using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Payments.DTOs;
using WealthMap.Domain.Enums;

namespace WealthMap.Application.Features.Payments.Queries.GetPaymentsForTarget;

public class GetPaymentsForTargetHandler
    : IQueryHandler<GetPaymentsForTargetQuery, IReadOnlyList<PaymentDto>>
{
    private readonly IPaymentRepository _payments;
    private readonly ICreditCardRepository _cards;
    private readonly IDebtRepository _debts;
    private readonly IInstallmentPurchaseRepository _installments;

    public GetPaymentsForTargetHandler(
        IPaymentRepository payments,
        ICreditCardRepository cards,
        IDebtRepository debts,
        IInstallmentPurchaseRepository installments)
    {
        _payments = payments;
        _cards = cards;
        _debts = debts;
        _installments = installments;
    }

    public async Task<IReadOnlyList<PaymentDto>> Handle(
        GetPaymentsForTargetQuery request, CancellationToken ct)
    {
        // Confirm the target is the caller's before returning its history, so an
        // unknown or someone else's id answers 404 rather than an empty list.
        await EnsureTargetBelongsToUser(request, ct);

        var payments = await _payments.GetForTargetAsync(
            request.UserId, request.TargetType, request.TargetId, ct);

        return payments.Select(PaymentDto.FromEntity).ToList();
    }

    private async Task EnsureTargetBelongsToUser(GetPaymentsForTargetQuery request, CancellationToken ct)
    {
        var exists = request.TargetType switch
        {
            PaymentTargetType.CreditCard =>
                await _cards.GetByIdForUserAsync(request.TargetId, request.UserId, ct) is not null,
            PaymentTargetType.Debt =>
                await _debts.GetByIdForUserAsync(request.TargetId, request.UserId, ct) is not null,
            _ =>
                await _installments.GetByIdForUserAsync(request.TargetId, request.UserId, ct) is not null
        };

        if (!exists)
            throw new NotFoundException(request.TargetType.ToString(), request.TargetId);
    }
}