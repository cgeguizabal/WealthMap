using WealthMap.Application.Features.Reports.DTOs;

namespace WealthMap.Application.Common.Interfaces;

public interface IPdfReportGenerator
{
    /// <param name="locale">
    /// "es" renders the document in Spanish, anything else in English.
    /// </param>
    /// <remarks>
    /// A parameter rather than state on the generator: it is registered as a
    /// singleton, so a language held in a field would leak one user's locale into
    /// another user's report whenever two downloads overlapped.
    /// </remarks>
    byte[] GenerateMonthlyReport(MonthlyReportDto report, string? locale = null);
}
