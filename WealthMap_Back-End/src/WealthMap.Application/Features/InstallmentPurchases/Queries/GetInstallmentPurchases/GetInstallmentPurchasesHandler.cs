using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Common.Services;
using WealthMap.Application.Features.InstallmentPurchases.DTOs;

namespace WealthMap.Application.Features.InstallmentPurchases.Queries.GetInstallmentPurchases;

public class GetInstallmentPurchasesHandler
    : IQueryHandler<GetInstallmentPurchasesQuery, IReadOnlyList<InstallmentPurchaseDto>>
{
    private readonly IInstallmentPurchaseRepository _installments;
    private readonly InstallmentContextLoader _context;

    public GetInstallmentPurchasesHandler(
        IInstallmentPurchaseRepository installments, InstallmentContextLoader context)
    {
        _installments = installments;
        _context = context;
    }

    public async Task<IReadOnlyList<InstallmentPurchaseDto>> Handle(
        GetInstallmentPurchasesQuery request, CancellationToken ct)
    {
        var purchases = await _installments.GetAllForUserAsync(request.UserId, ct);
        return await _context.ToDtoListAsync(purchases, request.UserId, ct);
    }
}
