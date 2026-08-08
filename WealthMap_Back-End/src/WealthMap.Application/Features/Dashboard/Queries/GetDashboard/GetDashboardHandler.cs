using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Common.Services;
using WealthMap.Application.Features.Dashboard.DTOs;

namespace WealthMap.Application.Features.Dashboard.Queries.GetDashboard;

public class GetDashboardHandler : IQueryHandler<GetDashboardQuery, DashboardDto>
{
    private readonly FinancialSnapshotLoader _loader;

    public GetDashboardHandler(FinancialSnapshotLoader loader) => _loader = loader;

    public async Task<DashboardDto> Handle(GetDashboardQuery request, CancellationToken ct)
    {
        var snapshot = await _loader.LoadAsync(request.UserId, ct);
        return DashboardDto.FromSnapshot(snapshot);
    }
}
