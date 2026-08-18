using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Common.Services;

namespace WealthMap.Application.Features.Purchases.Commands.DeletePurchase;

/// <summary>
/// Removes a purchase that should never have been recorded, and everything it did.
/// </summary>
/// <remarks>
/// A real delete, not an archive — unlike accounts and cards (§6.11), which are
/// archived because history points at them. Nothing points at a purchase, and a
/// mistyped one is not history worth keeping; it is noise in every total it
/// touches.
///
/// The cost, accepted deliberately: the movement it wrote is destroyed, so the
/// audit trail no longer records that this purchase ever existed. The remaining
/// history is kept self-consistent — later movements are rebased so the running
/// balance still adds up — but the deletion itself leaves no trace.
/// </remarks>
public class DeletePurchaseHandler : ICommandHandler<DeletePurchaseCommand, bool>
{
    private readonly IPurchaseRepository _purchases;
    private readonly PurchaseEffects _effects;
    private readonly IUnitOfWork _unitOfWork;

    public DeletePurchaseHandler(
        IPurchaseRepository purchases, PurchaseEffects effects, IUnitOfWork unitOfWork)
    {
        _purchases = purchases;
        _effects = effects;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeletePurchaseCommand request, CancellationToken ct)
    {
        var purchase = await _purchases.GetByIdForUserAsync(request.Id, request.UserId, ct)
            ?? throw new NotFoundException("Purchase", request.Id);

        // One transaction: restoring the balance, removing the movement, rebasing
        // the rest of the history and dropping the row are one correction. Half of
        // that committed on its own would leave the account disagreeing with its
        // own movement list.
        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            await _effects.ReverseAsync(purchase, ct);
            _purchases.Remove(purchase);
        }, ct);

        return true;
    }
}
