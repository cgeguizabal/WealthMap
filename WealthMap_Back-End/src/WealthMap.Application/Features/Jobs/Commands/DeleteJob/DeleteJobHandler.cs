using WealthMap.Application.Common.Exceptions;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Common.Messaging;

namespace WealthMap.Application.Features.Jobs.Commands.DeleteJob;

public class DeleteJobHandler : ICommandHandler<DeleteJobCommand, bool>
{
    private readonly IJobRepository _jobs;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteJobHandler(IJobRepository jobs, IUnitOfWork unitOfWork)
    {
        _jobs = jobs;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteJobCommand request, CancellationToken ct)
    {
        var job = await _jobs.GetByIdForUserAsync(request.Id, request.UserId, ct)
            ?? throw new NotFoundException("Job", request.Id);

        _jobs.Remove(job);
        await _unitOfWork.SaveChangesAsync(ct);

        return true;
    }
}