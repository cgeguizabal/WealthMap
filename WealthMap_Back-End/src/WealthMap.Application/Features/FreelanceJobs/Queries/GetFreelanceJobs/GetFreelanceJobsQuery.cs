using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.FreelanceJobs.DTOs;

namespace WealthMap.Application.Features.FreelanceJobs.Queries.GetFreelanceJobs;

public record GetFreelanceJobsQuery(Guid UserId) : IQuery<IReadOnlyList<FreelanceJobDto>>;
