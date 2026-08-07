using WealthMap.Application.Common.Messaging;

namespace WealthMap.Application.Features.AdditionalIncomes.Commands.DeleteAdditionalIncome;

public record DeleteAdditionalIncomeCommand(Guid Id, Guid UserId) : ICommand<bool>;