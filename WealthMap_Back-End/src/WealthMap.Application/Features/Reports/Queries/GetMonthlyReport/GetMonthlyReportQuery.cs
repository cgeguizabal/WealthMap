using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Reports.DTOs;

namespace WealthMap.Application.Features.Reports.Queries.GetMonthlyReport;

/// <summary><paramref name="Month"/> is an ISO year-month, e.g. "2026-08".</summary>
public record GetMonthlyReportQuery(Guid UserId, string Month) : IQuery<MonthlyReportDto>;
