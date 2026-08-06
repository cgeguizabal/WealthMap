using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.CreditCards.DTOs;
using WealthMap.Domain.Entities;
using WealthMap.Domain.ValueObjects;

namespace WealthMap.Application.Features.CreditCards.Commands.CreateCreditCard;

public class CreateCreditCardHandler : ICommandHandler<CreateCreditCardCommand, CreditCardDto>
{
    private readonly ICreditCardRepository _cards;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCreditCardHandler(ICreditCardRepository cards, IUnitOfWork unitOfWork)
    {
        _cards = cards;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreditCardDto> Handle(CreateCreditCardCommand request, CancellationToken ct)
    {
        var card = new CreditCard(
            request.UserId,
            request.CardName,
            request.BankName,
            new Money(request.CreditLimit, request.Currency),
            request.AnnualInterestRate,
            request.PaymentDueDay,
            request.StatementCutoffDay);

        await _cards.AddAsync(card, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return CreditCardDto.FromEntity(card);
    }
}