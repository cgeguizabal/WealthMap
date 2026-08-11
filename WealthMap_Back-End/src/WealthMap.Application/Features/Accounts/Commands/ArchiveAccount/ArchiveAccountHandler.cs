using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;

namespace WealthMap.Application.Features.Accounts.Commands.ArchiveAccount;

/// <summary>
/// Removes the account from the user's view. Its movements, purchases and
/// payments are left untouched — deleting the row would cascade them away.
/// </summary>
public class ArchiveAccountHandler : ICommandHandler<ArchiveAccountCommand, bool>
{
    private readonly IAccountRepository _accounts;
    private readonly IUnitOfWork _unitOfWork;

    public ArchiveAccountHandler(IAccountRepository accounts, IUnitOfWork unitOfWork)
    {
        _accounts = accounts;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(ArchiveAccountCommand request, CancellationToken ct)
    {
        var account = await _accounts.GetByIdForUserAsync(request.Id, request.UserId, ct)
            ?? throw new NotFoundException("Account", request.Id);

        account.Archive();
        await _unitOfWork.SaveChangesAsync(ct);

        return true;
    }
}
