using CashrewardsOffers.Application.Common.Interfaces;
using CashrewardsOffers.Domain.Common;
using CashrewardsOffers.Domain.Entities;
using CashrewardsOffers.Domain.Enums;
using CashrewardsOffers.Domain.Events;
using Mapster;
using Serilog;
using System.Linq;
using System.Threading.Tasks;

namespace CashrewardsOffers.Infrastructure.Services
{
    public class EventMerchantInitialisationService : EventInitialisationService, IEventInitialisationService<MerchantInitial>
    {
        private readonly IMerchantsPersistenceContext _merchantsPersistenceContext;
        private readonly IEventOutboxPersistenceContext _eventOutboxPersistenceContext;

        public EventMerchantInitialisationService(
            IMerchantsPersistenceContext merchantsPersistenceContext,
            IEventOutboxPersistenceContext eventOutboxPersistenceContext)
        {
            _merchantsPersistenceContext = merchantsPersistenceContext;
            _eventOutboxPersistenceContext = eventOutboxPersistenceContext;
        }

        protected override async Task GenerateInitialEvents()
        {
            Log.Information("Starting generation of merchant initialisation events");

            var merchants = (await _merchantsPersistenceContext.GetAllMerchants())
                .Where(m => m.Client == Client.Cashrewards && m.PremiumClient == null)
                .ToList();

            foreach (var merchant in merchants)
            {
                await SendEvent<MerchantInitial>(merchant);
            }

            if (merchants.Any())
            {
                var last = merchants.Last();
                last.DomainEvents.Dequeue();
                await SendEvent<MerchantChanged>(last);
            }

            Log.Information("Completed generation of merchant initialisation events");
        }

        private async Task SendEvent<T>(Merchant merchant) where T : DomainEvent
        {
            merchant.RaiseEvent(merchant.Adapt<T>());
            foreach (var domainEvent in merchant.DomainEvents)
            {
                await _eventOutboxPersistenceContext.Append(domainEvent);
            }
        }
    }
}
