using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Reports.DTOs;

namespace WealthMap.Application.Features.Reports.Queries.GetMonthlyReport;

/// <summary>A month of the user's finances, bounded by their own calendar.</summary>
/// <param name="Month">An ISO year-month, e.g. "2026-08".</param>
/// <param name="TimeZone">
/// IANA zone id from the browser, e.g. "America/Guatemala". Decides where the
/// month begins and ends.
///
/// Without it the boundaries are UTC, and a purchase made at nine in the evening
/// on the 31st in Guatemala is already the 1st in UTC — so it lands in the next
/// month's report while every screen in the app shows it under this one. Null
/// falls back to UTC, which keeps older clients working and reproduces the old
/// behaviour exactly.
/// </param>
public record GetMonthlyReportQuery(
    Guid UserId,
    string Month,
    string? TimeZone = null) : IQuery<MonthlyReportDto>;
