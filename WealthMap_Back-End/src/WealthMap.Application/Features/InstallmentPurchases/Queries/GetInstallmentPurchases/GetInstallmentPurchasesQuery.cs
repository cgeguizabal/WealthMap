using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.InstallmentPurchases.DTOs;

namespace WealthMap.Application.Features.InstallmentPurchases.Queries.GetInstallmentPurchases;

public record GetInstallmentPurchasesQuery(Guid UserId) : IQuery<IReadOnlyList<InstallmentPurchaseDto>>;
