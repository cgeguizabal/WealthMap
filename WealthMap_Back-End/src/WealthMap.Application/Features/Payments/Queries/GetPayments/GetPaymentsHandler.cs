using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Common.Models;
using WealthMap.Application.Features.Payments.DTOs;
using WealthMap.Domain.Enums;

namespace WealthMap.Application.Features.Payments.Queries.GetPayments;

public class GetPaymentsHandler : IQueryHandler<GetPaymentsQuery, PagedResult<PaymentDto>>
{
    private readonly IPaymentRepository _payments;

    public GetPaymentsHandler(IPaymentRepository payments) => _payments = payments;

    public async Task<PagedResult<PaymentDto>> Handle(GetPaymentsQuery request, CancellationToken ct)
    {
        var from = request.From is null
            ? (DateTime?)null
            : DateTime.SpecifyKind(request.From.Value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);

        // 'to' names a day the caller expects included, so the exclusive bound is the next day.
        var toExclusive = request.To is null
            ? (DateTime?)null
            : DateTime.SpecifyKind(
                request.To.Value.AddDays(1).ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);

        PaymentTargetType? targetType =
            Enum.TryParse<PaymentTargetType>(request.TargetType, ignoreCase: true, out var parsed)
                ? parsed
                : null;

        var totalCount = await _payments.CountForUserAsync(
            request.UserId, from, toExclusive, targetType, ct);

        var items = await _payments.GetPagedForUserAsync(
            request.UserId, from, toExclusive, targetType, request.Page, request.PageSize, ct);

        return new PagedResult<PaymentDto>(
            items.Select(PaymentDto.FromEntity).ToList(),
            request.Page,
            request.PageSize,
            totalCount);
    }
}