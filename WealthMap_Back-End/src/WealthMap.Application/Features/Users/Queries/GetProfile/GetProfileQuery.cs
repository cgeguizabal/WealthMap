using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Users.DTOs;

namespace WealthMap.Application.Features.Users.Queries.GetProfile;

public record GetProfileQuery(Guid UserId) : IQuery<UserProfileDto>;
