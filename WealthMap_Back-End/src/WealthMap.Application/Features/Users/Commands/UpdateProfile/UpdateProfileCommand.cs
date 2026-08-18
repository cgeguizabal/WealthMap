using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Users.DTOs;

namespace WealthMap.Application.Features.Users.Commands.UpdateProfile;

public record UpdateProfileCommand(
    Guid UserId,
    string FullName,
    string Country,
    string Currency) : ICommand<UserProfileDto>;
