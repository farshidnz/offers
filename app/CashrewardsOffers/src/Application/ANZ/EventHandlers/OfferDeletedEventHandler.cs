using CashrewardsOffers.Application.ANZ.Services;
using CashrewardsOffers.Application.Common.Models;
using CashrewardsOffers.Domain.Events;
using MassTransit;
using Serilog;
using System.Threading.Tasks;

namespace CashrewardsOffers.Application.ANZ.EventHandlers
{
    public class OfferDeletedEventHandler : IConsumer<DomainEventNotification<OfferDeleted>>
    {
        private readonly IAnzUpdateService _anzUpdateService;

        public OfferDeletedEventHandler(IAnzUpdateService anzUpdateService)
        {
            _anzUpdateService = anzUpdateService;
        }

        public async Task Consume(ConsumeContext<DomainEventNotification<OfferDeleted>> context)
        {
            var domainEvent = context.Message.DomainEvent;

            Log.Information("Received {EventType} event {EventId} for client: {Client} merchant: {MerchantHyphenatedString} offer: {HyphenatedString}",
                domainEvent.GetType().Name,
                domainEvent.Metadata.EventID,
                domainEvent.Client,
                domainEvent.Merchant.HyphenatedString,
                domainEvent.HyphenatedString);

            await _anzUpdateService.DeleteOffer(domainEvent);
        }
    }
}
