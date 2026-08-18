using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Accounts.DTOs;
using WealthMap.Domain.Enums;

namespace WealthMap.Application.Features.Accounts.Commands.UpdateAccountTracking;

public class UpdateAccountTrackingHandler : ICommandHandler<UpdateAccountTrackingCommand, AccountDto>
{
    private readonly IAccountRepository _accounts;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAccountTrackingHandler(IAccountRepository accounts, IUnitOfWork unitOfWork)
    {
        _accounts = accounts;
        _unitOfWork = unitOfWork;
    }

    public async Task<AccountDto> Handle(UpdateAccountTrackingCommand request, CancellationToken ct)
    {
        var account = await _accounts.GetByIdForUserAsync(request.Id, request.UserId, ct)
            ?? throw new NotFoundException("Account", request.Id);

        var mode = (TrackingMode)request.TrackingMode;

        // Order matters both ways, so the safe move is made first each time.
        // Turning sync ON: the digits must land before the mode, or the mode change
        // is refused. Turning sync OFF: the mode must drop before the digits are
        // cleared, or clearing them is refused. Doing it in one order for both cases
        // would reject one of them.
        if (mode == TrackingMode.Manual)
        {
            account.SetTrackingMode(mode);
            account.SetLastFour(request.LastFour);
        }
        else
        {
            account.SetLastFour(request.LastFour);
            account.SetTrackingMode(mode);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        return AccountDto.FromEntity(account);
    }
}
