using CashrewardsOffers.Domain.Common;
using System.Threading.Tasks;

namespace CashrewardsOffers.Application.Common.Interfaces
{
    public interface IEventInitialisationService<T> where T : DomainEvent
    {
        bool IsTriggered { get; set; }

        bool IsRunning { get; }

        Task CheckForInitialisationRequests();
    }
}
