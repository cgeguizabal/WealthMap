using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.FreelanceJobs.DTOs;

namespace WealthMap.Application.Features.FreelanceJobs.Queries.GetFreelanceJobById;

public record GetFreelanceJobByIdQuery(Guid Id, Guid UserId) : IQuery<FreelanceJobDto>;
