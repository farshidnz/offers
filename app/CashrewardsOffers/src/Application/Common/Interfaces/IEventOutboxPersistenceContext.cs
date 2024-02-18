using CashrewardsOffers.Domain.Common;
using System;
using System.Threading.Tasks;

namespace CashrewardsOffers.Application.Common.Interfaces
{
    public interface IEventOutboxPersistenceContext
    {
        Task Append(DomainEvent domainEvent);
        Task Delete(Guid eventId);
        Task<DomainEvent> GetNext();
    }
}
