using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Common.Services;
using WealthMap.Application.Features.CreditCards.DTOs;
using WealthMap.Domain.Entities;
using WealthMap.Domain.Enums;
using WealthMap.Domain.ValueObjects;

namespace WealthMap.Application.Features.CreditCards.Commands.CreateCreditCard;

public class CreateCreditCardHandler : ICommandHandler<CreateCreditCardCommand, CreditCardDto>
{
    private readonly ICreditCardRepository _cards;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CardStatementLoader _statements;

    public CreateCreditCardHandler(
        ICreditCardRepository cards, IUnitOfWork unitOfWork, CardStatementLoader statements)
    {
        _cards = cards;
        _unitOfWork = unitOfWork;
        _statements = statements;
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

        // Digits before mode: SetTrackingMode refuses EmailSync while LastFour is
        // still null, so the reverse order would reject a valid request.
        if (request.LastFour is not null)
            card.SetLastFour(request.LastFour);

        if (request.TrackingMode is { } mode)
            card.SetTrackingMode((TrackingMode)mode);

        await _cards.AddAsync(card, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return await _statements.ToDtoAsync(card, request.UserId, ct);
    }
}