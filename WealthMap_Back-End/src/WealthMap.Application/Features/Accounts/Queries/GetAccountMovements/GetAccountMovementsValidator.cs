using FluentValidation;

namespace WealthMap.Application.Features.Accounts.Queries.GetAccountMovements;

public class GetAccountMovementsValidator : AbstractValidator<GetAccountMovementsQuery>
{
    public GetAccountMovementsValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be 1 or greater.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");
    }
}