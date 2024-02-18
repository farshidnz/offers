using System;

namespace CashrewardsOffers.Application.Common.Interfaces
{
    public interface IDateTime
    {
        public DateTime Now { get; }
        DateTime UtcNow { get; }
    }
}