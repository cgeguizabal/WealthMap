using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Users.DTOs;

namespace WealthMap.Application.Features.Users.Queries.GetProfile;

public class GetProfileHandler : IQueryHandler<GetProfileQuery, UserProfileDto>
{
    private readonly IUserRepository _users;

    public GetProfileHandler(IUserRepository users) => _users = users;

    public async Task<UserProfileDto> Handle(GetProfileQuery request, CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(request.UserId, ct)
            ?? throw new NotFoundException("User", request.UserId);

        return UserProfileDto.FromEntity(user);
    }
}
