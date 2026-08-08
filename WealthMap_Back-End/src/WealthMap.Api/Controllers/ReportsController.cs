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

    [HttpGet("monthly/{month}/pdf")]
    public async Task<IActionResult> GetMonthlyPdf(string month, CancellationToken ct)
    {
        var report = await _sender.Send(new GetMonthlyReportQuery(User.GetUserId(), month), ct);
        var bytes = _pdf.GenerateMonthlyReport(report);

        return File(bytes, "application/pdf", $"wealthmap-{month}.pdf");
    }
}
