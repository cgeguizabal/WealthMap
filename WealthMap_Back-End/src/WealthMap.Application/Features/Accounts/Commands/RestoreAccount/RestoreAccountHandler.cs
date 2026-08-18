using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;

namespace WealthMap.Application.Features.Accounts.Commands.RestoreAccount;

/// <summary>
/// Undoes an archive. The mirror of ArchiveAccountHandler, and the reason
/// archiving is safe to offer at all: without this, a mis-click removed an
/// account from every list and total with no way back.
/// </summary>
public class RestoreAccountHandler : ICommandHandler<RestoreAccountCommand, bool>
{
    private readonly IAccountRepository _accounts;
    private readonly IUnitOfWork _unitOfWork;

    public RestoreAccountHandler(IAccountRepository accounts, IUnitOfWork unitOfWork)
    {
        _accounts = accounts;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(RestoreAccountCommand request, CancellationToken ct)
    {
        // Archived rows are excluded from the default lookup, so this asks for
        // them explicitly — the whole point is to find one that is hidden.
        var account = await _accounts.GetByIdForUserAsync(request.Id, request.UserId, ct)
            ?? throw new NotFoundException("Account", request.Id);

        account.Restore();
        await _unitOfWork.SaveChangesAsync(ct);

        return true;
    }
}
