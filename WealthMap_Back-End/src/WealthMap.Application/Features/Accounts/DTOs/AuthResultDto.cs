namespace WealthMap.Application.Features.Auth.DTOs;

// Currency travels with the session so the client can format money on the very
// first render, before any account has loaded. It is the user's display currency,
// not a per-account one.
public record AuthResultDto(
    Guid UserId, string Email, string FullName, string Currency, string Token);