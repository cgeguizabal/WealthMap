using FluentValidation;
using WealthMap.Application.Common.Models;

namespace WealthMap.Application.Features.Notifications.Queries.GetNotifications;

public class GetNotificationsValidator : AbstractValidator<GetNotificationsQuery>
{
    public GetNotificationsValidator()
    {
        this.ApplyPagingRules();
    }
}
