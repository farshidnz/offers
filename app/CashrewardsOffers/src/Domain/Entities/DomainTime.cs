using System;

namespace CashrewardsOffers.Domain.Entities
{
    public interface IDomainTime
    {
        DateTimeOffset UtcNow { get; }
    }

    public class DateTimeOffsetWrapper : IDomainTime
    {
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}
