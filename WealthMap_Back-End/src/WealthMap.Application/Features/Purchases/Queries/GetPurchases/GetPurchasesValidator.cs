using FluentValidation;
using WealthMap.Application.Common.Models;

namespace WealthMap.Application.Features.Purchases.Queries.GetPurchases;

public class GetPurchasesValidator : AbstractValidator<GetPurchasesQuery>
{
    public GetPurchasesValidator()
    {
        this.ApplyPagingRules();

        RuleFor(x => x.Year)
            .InclusiveBetween(2000, 2100).WithMessage("Year must be between 2000 and 2100.")
            .When(x => x.Year is not null);

        RuleFor(x => x.Month)
            .InclusiveBetween(1, 12).WithMessage("Month must be between 1 and 12.")
            .When(x => x.Month is not null);

        RuleFor(x => x.Year)
            .NotNull().WithMessage("Filtering by month requires a year.")
            .When(x => x.Month is not null);
    }
}
