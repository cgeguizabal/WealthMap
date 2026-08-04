namespace WealthMap.Application.Features.Auth.DTOs;

public record AuthResultDto(Guid UserId, string Email, string FullName, string Token);