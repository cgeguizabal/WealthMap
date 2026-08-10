using FluentValidation;
using WealthMap.Application.Common.Models;
using WealthMap.Domain.Enums;

namespace WealthMap.Application.Features.Payments.Queries.GetPayments;

public class GetPaymentsValidator : AbstractValidator<GetPaymentsQuery>
{
    public GetPaymentsValidator()
    {
        this.ApplyPagingRules();

        RuleFor(x => x.TargetType)
            .Must(t => Enum.TryParse<PaymentTargetType>(t, ignoreCase: true, out _))
            .WithMessage("Target type must be 'CreditCard', 'Debt' or 'Installment'.")
            .When(x => !string.IsNullOrWhiteSpace(x.TargetType));

        RuleFor(x => x.To)
            .GreaterThanOrEqualTo(x => x.From!.Value)
            .WithMessage("'to' cannot be earlier than 'from'.")
            .When(x => x.From is not null && x.To is not null);
    }
}