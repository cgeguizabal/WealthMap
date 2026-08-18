using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Common.Services;
using WealthMap.Application.Features.InstallmentPurchases.DTOs;

namespace WealthMap.Application.Features.InstallmentPurchases.Queries.GetInstallmentPurchaseById;

public class GetInstallmentPurchaseByIdHandler
    : IQueryHandler<GetInstallmentPurchaseByIdQuery, InstallmentPurchaseDto>
{
    private readonly IInstallmentPurchaseRepository _installments;
    private readonly InstallmentContextLoader _context;

    public GetInstallmentPurchaseByIdHandler(
        IInstallmentPurchaseRepository installments, InstallmentContextLoader context)
    {
        _installments = installments;
        _context = context;
    }

    public async Task<InstallmentPurchaseDto> Handle(
        GetInstallmentPurchaseByIdQuery request, CancellationToken ct)
    {
        var purchase = await _installments.GetByIdForUserAsync(request.Id, request.UserId, ct)
            ?? throw new NotFoundException("InstallmentPurchase", request.Id);

        return await _context.ToDtoAsync(purchase, request.UserId, ct);
    }
}
