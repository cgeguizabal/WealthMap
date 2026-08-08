using System.Globalization;
using FluentValidation;

namespace WealthMap.Application.Features.Reports.Queries.GetMonthlyReport;

public class GetMonthlyReportValidator : AbstractValidator<GetMonthlyReportQuery>
{
    public GetMonthlyReportValidator()
    {
        RuleFor(x => x.Month)
            .NotEmpty().WithMessage("Month is required.")
            .Must(BeAnIsoMonth).WithMessage("Month must be in yyyy-MM format, e.g. 2026-08.");
    }

    private static bool BeAnIsoMonth(string month) =>
        DateTime.TryParseExact(
            month, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
}
