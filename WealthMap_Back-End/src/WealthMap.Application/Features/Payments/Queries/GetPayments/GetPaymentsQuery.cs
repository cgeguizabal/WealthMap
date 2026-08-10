using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Common.Models;
using WealthMap.Application.Features.Payments.DTOs;

namespace WealthMap.Application.Features.Payments.Queries.GetPayments;

/// <summary><paramref name="To"/> is inclusive of the whole day; the handler widens it.</summary>
public record GetPaymentsQuery(
    Guid UserId,
    DateOnly? From = null,
    DateOnly? To = null,
    string? TargetType = null,
    int Page = 1,
    int PageSize = PagedQueryRules.DefaultPageSize)
    : IQuery<PagedResult<PaymentDto>>, IPagedQuery;