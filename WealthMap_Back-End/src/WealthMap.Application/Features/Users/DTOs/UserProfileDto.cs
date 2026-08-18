using WealthMap.Domain.Entities;

namespace WealthMap.Application.Features.Users.DTOs;

/// <summary>
/// The account holder's own details. Never anyone else's — every route that
/// returns this reads the id from the token.
/// </summary>
public record UserProfileDto(
    Guid Id,
    string Email,
    string FullName,
    string Country,
    string Currency,
    DateTime? TermsAcceptedAt,
    string? AcceptedPolicyVersion,
    DateTime CreatedAt)
{
    public static UserProfileDto FromEntity(User user) => new(
        user.Id,
        user.Email,
        user.FullName,
        user.Country,
        user.Currency,
        user.TermsAcceptedAt,
        user.AcceptedPolicyVersion,
        user.CreatedAt);
}
