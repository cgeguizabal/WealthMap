using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Alerts.DTOs;

namespace WealthMap.Application.Features.Alerts.Queries.GetAlerts;

public record GetAlertsQuery(Guid UserId) : IQuery<IReadOnlyList<AlertDto>>;
