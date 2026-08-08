using FluentValidation;
using WealthMap.Application.Common.Models;

namespace WealthMap.Application.Features.Accounts.Queries.GetAccountMovements;

public class GetAccountMovementsValidator : AbstractValidator<GetAccountMovementsQuery>
{
    public GetAccountMovementsValidator()
    {
        this.ApplyPagingRules();
    }
}
