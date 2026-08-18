using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Users.DTOs;

namespace WealthMap.Application.Features.Users.Commands.UpdateProfile;

public class UpdateProfileHandler : ICommandHandler<UpdateProfileCommand, UserProfileDto>
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProfileHandler(IUserRepository users, IUnitOfWork unitOfWork)
    {
        _users = users;
        _unitOfWork = unitOfWork;
    }

    public async Task<UserProfileDto> Handle(UpdateProfileCommand request, CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(request.UserId, ct)
            ?? throw new NotFoundException("User", request.UserId);

        user.UpdateProfile(request.FullName, request.Country, request.Currency);

        await _unitOfWork.SaveChangesAsync(ct);

        return UserProfileDto.FromEntity(user);
    }
}
