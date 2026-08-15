using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.BankDefaults.DTOs;

namespace WealthMap.Application.Features.BankDefaults.Queries.GetBankDefaults;

public class GetBankDefaultsHandler
    : IQueryHandler<GetBankDefaultsQuery, IReadOnlyList<BankDefaultDto>>
{
    private readonly IBankDefaultRepository _bankDefaults;
    private readonly IAccountRepository _accounts;

    public GetBankDefaultsHandler(IBankDefaultRepository bankDefaults, IAccountRepository accounts)
    {
        _bankDefaults = bankDefaults;
        _accounts = accounts;
    }

    public async Task<IReadOnlyList<BankDefaultDto>> Handle(
        GetBankDefaultsQuery request, CancellationToken ct)
    {
        var defaults = await _bankDefaults.GetAllForUserAsync(request.UserId, ct);

        if (defaults.Count == 0) return [];

        // includeArchived: a default may point at an account archived after it was
        // set — the FK is Restrict, so the row is still valid and must still render
        // with a name rather than falling back to "(unknown)".
        var accounts = await _accounts.GetAllForUserAsync(request.UserId, includeArchived: true, ct: ct);
        var names = accounts.ToDictionary(a => a.Id, a => a.Name);

        return defaults
            .Select(d => BankDefaultDto.FromEntity(
                d, names.TryGetValue(d.DefaultAccountId, out var name) ? name : string.Empty))
            .ToList();
    }
}
