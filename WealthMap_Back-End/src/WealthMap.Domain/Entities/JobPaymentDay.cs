using WealthMap.Domain.Common;
using WealthMap.Domain.Exceptions;

namespace WealthMap.Domain.Entities;

public class JobPaymentDay : BaseEntity
{
    public Guid JobId { get; private set; }
    public int DayOfMonth { get; private set; }

    private JobPaymentDay() { }

    internal JobPaymentDay(Guid jobId, int dayOfMonth)
    {
        if (dayOfMonth is < 1 or > 31)
            throw new DomainException("Payment day must be between 1 and 31.");

        JobId = jobId;
        DayOfMonth = dayOfMonth;
    }
}