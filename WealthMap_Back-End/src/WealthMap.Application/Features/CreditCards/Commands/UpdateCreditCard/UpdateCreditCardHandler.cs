using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Common.Services;
using WealthMap.Application.Features.CreditCards.DTOs;

namespace WealthMap.Application.Features.CreditCards.Commands.UpdateCreditCard;

public class UpdateCreditCardHandler : ICommandHandler<UpdateCreditCardCommand, CreditCardDto>
{
    private readonly ICreditCardRepository _cards;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CardStatementLoader _statements;

    public UpdateCreditCardHandler(
        ICreditCardRepository cards, IUnitOfWork unitOfWork, CardStatementLoader statements)
    {
        _cards = cards;
        _unitOfWork = unitOfWork;
        _statements = statements;
    }

    public async Task<CreditCardDto> Handle(UpdateCreditCardCommand request, CancellationToken ct)
    {
        var card = await _cards.GetByIdForUserAsync(request.Id, request.UserId, ct)
            ?? throw new NotFoundException("CreditCard", request.Id);

        card.UpdateDetails(
            request.CardName,
            request.BankName,
            request.AnnualInterestRate,
            request.PaymentDueDay,
            request.StatementCutoffDay,
            request.Notes);

        await _unitOfWork.SaveChangesAsync(ct);

        return await _statements.ToDtoAsync(card, request.UserId, ct);
    }
}