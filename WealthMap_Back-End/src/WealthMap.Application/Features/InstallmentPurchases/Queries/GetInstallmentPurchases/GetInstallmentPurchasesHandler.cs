using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.InstallmentPurchases.DTOs;

namespace WealthMap.Application.Features.InstallmentPurchases.Queries.GetInstallmentPurchases;

public class GetInstallmentPurchasesHandler
    : IQueryHandler<GetInstallmentPurchasesQuery, IReadOnlyList<InstallmentPurchaseDto>>
{
    private readonly IInstallmentPurchaseRepository _installments;

    public GetInstallmentPurchasesHandler(IInstallmentPurchaseRepository installments) =>
        _installments = installments;

    public async Task<IReadOnlyList<InstallmentPurchaseDto>> Handle(
        GetInstallmentPurchasesQuery request, CancellationToken ct)
    {
        var purchases = await _installments.GetAllForUserAsync(request.UserId, ct);
        return purchases.Select(InstallmentPurchaseDto.FromEntity).ToList();
    }
}
