using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Payments.DTOs;
using WealthMap.Domain.Enums;

namespace WealthMap.Application.Features.Payments.Queries.GetPaymentsForTarget;

public record GetPaymentsForTargetQuery(
    Guid UserId,
    PaymentTargetType TargetType,
    Guid TargetId) : IQuery<IReadOnlyList<PaymentDto>>;