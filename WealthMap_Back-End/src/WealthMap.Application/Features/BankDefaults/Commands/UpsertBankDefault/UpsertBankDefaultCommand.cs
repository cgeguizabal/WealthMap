using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.BankDefaults.DTOs;

namespace WealthMap.Application.Features.BankDefaults.Commands.UpsertBankDefault;

/// <summary>
/// Creates or replaces the default for one bank and direction.
/// </summary>
/// <remarks>
/// An upsert rather than separate create and update endpoints because the key is
/// the pair, not an id: the user thinks "outbound transfers from BAC come from
/// this account", and saying that twice should mean the same thing as saying it
/// once, not fail with a duplicate.
/// </remarks>
public record UpsertBankDefaultCommand(
    Guid UserId,
    string BankName,
    int Direction,
    Guid DefaultAccountId) : ICommand<BankDefaultDto>;
