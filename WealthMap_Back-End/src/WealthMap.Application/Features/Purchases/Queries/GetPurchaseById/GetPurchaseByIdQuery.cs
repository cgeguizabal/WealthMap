using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Purchases.DTOs;

namespace WealthMap.Application.Features.Purchases.Queries.GetPurchaseById;

public record GetPurchaseByIdQuery(Guid Id, Guid UserId) : IQuery<PurchaseDto>;
