using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Dashboard.DTOs;

namespace WealthMap.Application.Features.Dashboard.Queries.GetDashboard;

public record GetDashboardQuery(Guid UserId) : IQuery<DashboardDto>;
