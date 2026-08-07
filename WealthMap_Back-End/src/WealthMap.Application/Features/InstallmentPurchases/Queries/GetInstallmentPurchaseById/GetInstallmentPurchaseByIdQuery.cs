using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.InstallmentPurchases.DTOs;

namespace WealthMap.Application.Features.InstallmentPurchases.Queries.GetInstallmentPurchaseById;

public record GetInstallmentPurchaseByIdQuery(Guid Id, Guid UserId) : IQuery<InstallmentPurchaseDto>;
