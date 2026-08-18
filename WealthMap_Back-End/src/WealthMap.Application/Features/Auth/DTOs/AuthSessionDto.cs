namespace WealthMap.Application.Features.Auth.DTOs;

/// <summary>
/// The full result of authenticating: the part the client is shown, and the
/// refresh token, which is not part of it.
/// </summary>
/// <remarks>
/// They are separated here so the split is impossible to get wrong further out.
/// <see cref="Result"/> is what becomes the JSON body; <see cref="RefreshToken"/>
/// is handed to the API layer to be written as an httpOnly cookie and must never
/// be serialised into a response — putting it in the body would hand it straight
/// back to the JavaScript it is meant to be hidden from.
/// </remarks>
public record AuthSessionDto(AuthResultDto Result, string RefreshToken);
