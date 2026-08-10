using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.InstallmentPurchases.DTOs;

namespace WealthMap.Application.Features.InstallmentPurchases.Commands.PayInstallment;

public record PayInstallmentCommand(
    Guid InstallmentPurchaseId,
    Guid UserId,
    string SourceType,
    Guid? SourceAccountId,
    string? Notes = null) : ICommand<InstallmentPaymentResultDto>;
