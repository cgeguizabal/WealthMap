using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WealthMap.Api.Extensions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Reports.Queries.GetMonthlyReport;

namespace WealthMap.Api.Controllers;

[ApiController]
[Route("api/v1/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IPdfReportGenerator _pdf;

    public ReportsController(ISender sender, IPdfReportGenerator pdf)
    {
        _sender = sender;
        _pdf = pdf;
    }

    [HttpGet("monthly/{month}")]
    public async Task<IActionResult> GetMonthly(string month, CancellationToken ct)
    {
        var result = await _sender.Send(new GetMonthlyReportQuery(User.GetUserId(), month), ct);
        return Ok(result);
    }

    /// <summary>
    /// The report as a PDF, rendered in <paramref name="lang"/>.
    /// </summary>
    /// <param name="lang">
    /// "es" for Spanish, anything else English. Sent by the client from its own
    /// language selector — the server has no other way to know: a JWT carries no
    /// locale, and a stored preference would be a second place the choice lives.
    /// </param>
    /// <remarks>
    /// A query parameter rather than <c>Accept-Language</c>, because the language
    /// wanted here is the one chosen *in the app*, which is not necessarily the
    /// browser's. A user reading WealthMap in Spanish on an English-configured
    /// machine should get a Spanish report.
    /// </remarks>
    [HttpGet("monthly/{month}/pdf")]
    public async Task<IActionResult> GetMonthlyPdf(
        string month,
        CancellationToken ct,
        [FromQuery] string? lang = null)
    {
        var report = await _sender.Send(new GetMonthlyReportQuery(User.GetUserId(), month), ct);
        var bytes = _pdf.GenerateMonthlyReport(report, lang);

        return File(bytes, "application/pdf", $"wealthmap-{month}.pdf");
    }
}
