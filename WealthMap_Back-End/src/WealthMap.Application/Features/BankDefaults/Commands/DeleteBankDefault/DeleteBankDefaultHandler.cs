using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;

namespace WealthMap.Application.Features.BankDefaults.Commands.DeleteBankDefault;

/// <summary>
/// A real delete, not an archive. A bank default holds no history and is
/// referenced by nothing — removing it loses no information the user would ever
/// want back, which is the test §6.11 applies before archiving anything.
/// </summary>
public class DeleteBankDefaultHandler : ICommandHandler<DeleteBankDefaultCommand, bool>
{
    private readonly IBankDefaultRepository _bankDefaults;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteBankDefaultHandler(IBankDefaultRepository bankDefaults, IUnitOfWork unitOfWork)
    {
        _bankDefaults = bankDefaults;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteBankDefaultCommand request, CancellationToken ct)
    {
        var bankDefault = await _bankDefaults.GetByIdForUserAsync(request.Id, request.UserId, ct)
            ?? throw new NotFoundException("BankDefault", request.Id);

        _bankDefaults.Remove(bankDefault);
        await _unitOfWork.SaveChangesAsync(ct);

        return true;
    }
}
