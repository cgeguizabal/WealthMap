using WealthMap.Application.Features.Reports.DTOs;

namespace WealthMap.Application.Common.Interfaces;

public interface IPdfReportGenerator
{
    byte[] GenerateMonthlyReport(MonthlyReportDto report);
}
