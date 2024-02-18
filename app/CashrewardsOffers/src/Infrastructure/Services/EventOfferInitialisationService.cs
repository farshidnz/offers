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
    public class EventOfferInitialisationService : EventInitialisationService, IEventInitialisationService<OfferInitial>
    {
        private readonly IOffersPersistenceContext _offersPersistenceContext;
        private readonly IEventOutboxPersistenceContext _eventOutboxPersistenceContext;

        public EventOfferInitialisationService(
            IOffersPersistenceContext offersPersistenceContext,
            IEventOutboxPersistenceContext eventOutboxPersistenceContext)
        {
            _offersPersistenceContext = offersPersistenceContext;
            _eventOutboxPersistenceContext = eventOutboxPersistenceContext;
        }

        protected override async Task GenerateInitialEvents()
        {
            Log.Information("Starting generation of offer initialisation events");

            var offers = (await _offersPersistenceContext.GetAllOffers())
                .Where(m => m.Client == Client.Cashrewards && m.PremiumClient == null)
                .ToList();

            foreach (var offer in offers)
            {
                await SendEvent<OfferInitial>(offer);
            }

            if (offers.Any())
            {
                var last = offers.Last();
                last.DomainEvents.Dequeue();
                await SendEvent<OfferChanged>(last);
            }

            Log.Information("Completed generation of offer initialisation events");
        }

        private async Task SendEvent<T>(Offer offer) where T : DomainEvent
        {
            offer.RaiseEvent(offer.Adapt<T>());
            foreach (var domainEvent in offer.DomainEvents)
            {
                await _eventOutboxPersistenceContext.Append(domainEvent);
            }
        }
    }
}
