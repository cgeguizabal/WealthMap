using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.InstallmentPurchases.DTOs;

namespace WealthMap.Application.Features.InstallmentPurchases.Queries.GetInstallmentPurchaseById;

public class GetInstallmentPurchaseByIdHandler
    : IQueryHandler<GetInstallmentPurchaseByIdQuery, InstallmentPurchaseDto>
{
    private readonly IInstallmentPurchaseRepository _installments;

    public GetInstallmentPurchaseByIdHandler(IInstallmentPurchaseRepository installments) =>
        _installments = installments;

    public async Task<InstallmentPurchaseDto> Handle(
        GetInstallmentPurchaseByIdQuery request, CancellationToken ct)
    {
        var purchase = await _installments.GetByIdForUserAsync(request.Id, request.UserId, ct)
            ?? throw new NotFoundException("InstallmentPurchase", request.Id);

        return InstallmentPurchaseDto.FromEntity(purchase);
    }
}
