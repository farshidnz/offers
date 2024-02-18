using CashrewardsOffers.Application.ANZ.Services;
using CashrewardsOffers.Application.Common.Interfaces;
using CashrewardsOffers.Application.Common.Models;
using CashrewardsOffers.Domain.Entities;
using CashrewardsOffers.Domain.Enums;
using CashrewardsOffers.Domain.Events;
using MassTransit;
using Serilog;
using System.Threading.Tasks;

namespace CashrewardsOffers.Application.ANZ.EventHandlers
{
    public class OfferChangedEventHandler : IConsumer<DomainEventNotification<OfferChanged>>, IConsumer<DomainEventNotification<OfferInitial>>
    {
        private readonly IAnzUpdateService _anzUpdateService;

        public OfferChangedEventHandler(IAnzUpdateService anzUpdateService)
        {
            _anzUpdateService = anzUpdateService;
        }

        public async Task Consume(ConsumeContext<DomainEventNotification<OfferChanged>> context)
        {
            await DoConsume(context.Message.DomainEvent);
        }

        public async Task Consume(ConsumeContext<DomainEventNotification<OfferInitial>> context)
        {
            await DoConsume(context.Message.DomainEvent);
        }

        public async Task DoConsume(OfferEventBase domainEvent)
        {
            Log.Information("Received {EventType} event {EventId} for client: {Client} merchant: {MerchantHyphenatedString} offer: {HyphenatedString}",
                domainEvent.GetType().Name,
                domainEvent.Metadata.EventID,
                domainEvent.Client,
                domainEvent.Merchant.HyphenatedString,
                domainEvent.HyphenatedString);

            await _anzUpdateService.UpdateOffer(domainEvent);
        }
    }
}
