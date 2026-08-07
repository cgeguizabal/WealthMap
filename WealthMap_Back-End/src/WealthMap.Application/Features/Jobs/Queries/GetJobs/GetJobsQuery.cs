using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Jobs.DTOs;

namespace WealthMap.Application.Features.Jobs.Queries.GetJobs;

public record GetJobsQuery(Guid UserId) : IQuery<IReadOnlyList<JobDto>>;