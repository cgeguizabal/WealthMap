using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Common.Services;
using WealthMap.Application.Features.Alerts.DTOs;

namespace WealthMap.Application.Features.Alerts.Queries.GetAlerts;

public class GetAlertsHandler : IQueryHandler<GetAlertsQuery, IReadOnlyList<AlertDto>>
{
    private readonly FinancialSnapshotLoader _loader;

    public GetAlertsHandler(FinancialSnapshotLoader loader) => _loader = loader;

    public async Task<IReadOnlyList<AlertDto>> Handle(GetAlertsQuery request, CancellationToken ct)
    {
        var snapshot = await _loader.LoadAsync(request.UserId, ct);
        return AlertRules.Evaluate(snapshot);
    }
}
