using WealthMap.Domain.Entities;

namespace WealthMap.Application.Features.Jobs.DTOs;

public record DeductionDto(
    Guid Id,
    string Name,
    string Type,
    decimal Value)
{
    public static DeductionDto FromEntity(Deduction deduction) => new(
        deduction.Id,
        deduction.Name,
        deduction.Type.ToString(),
        deduction.Value);
}