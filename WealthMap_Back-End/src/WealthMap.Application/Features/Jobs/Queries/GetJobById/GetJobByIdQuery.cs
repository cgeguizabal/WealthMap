using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Jobs.DTOs;

namespace WealthMap.Application.Features.Jobs.Queries.GetJobById;

public record GetJobByIdQuery(Guid Id, Guid UserId) : IQuery<JobDto>;