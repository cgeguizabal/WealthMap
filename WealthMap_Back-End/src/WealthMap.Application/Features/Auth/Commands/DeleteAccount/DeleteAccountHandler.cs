using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Domain.Exceptions;

namespace WealthMap.Application.Features.Auth.Commands.DeleteAccount;

public class DeleteAccountHandler : ICommandHandler<DeleteAccountCommand, bool>
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUserEraser _eraser;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteAccountHandler(
        IUserRepository users,
        IPasswordHasher passwordHasher,
        IUserEraser eraser,
        IUnitOfWork unitOfWork)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _eraser = eraser;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteAccountCommand request, CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(request.UserId, ct)
            ?? throw new NotFoundException("User", request.UserId);

        // Deliberately the same wording as a failed sign-in. Someone holding a
        // stolen token learns nothing from the difference, and the account holder
        // knows perfectly well which password they just typed.
        if (!_passwordHasher.Verify(user.PasswordHash, request.Password))
            throw new DomainException("Incorrect password.");

        // One transaction: the eraser verifies afterwards that nothing survived,
        // and throwing there must roll the whole thing back. A half-deleted
        // account is worse than one that is still there.
        await _unitOfWork.ExecuteInTransactionAsync(
            async () => await _eraser.EraseAsync(request.UserId, ct), ct);

        return true;
    }
}
